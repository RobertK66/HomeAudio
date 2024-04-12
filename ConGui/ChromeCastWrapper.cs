using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Receiver;
using Sharpcaster.Models;
using Sharpcaster.Models.ChromecastStatus;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using MediaStatus = QueueCaster.MediaStatus;

namespace ConGui {
    public class CCWStatusEventArgs : EventArgs {
        public CCWStatusEventArgs(ChromecastStatus? status, MediaStatus? medStatus, bool currentFirstTrack, bool currentLastTrack) {
            AppCount = status?.Applications?.Count ?? 0;
            if (AppCount > 0) {
                AppID = status?.Applications[0].AppId ?? "";
                AppName = status?.Applications[0].DisplayName ?? "";
                AppStatus = status?.Applications[0].StatusText ?? "";
                
            } else {
                AppID = "";
                AppName = "";
                AppStatus = "";
            }
            VolumeLevel = status?.Volume.Level ?? 0.0;
            if (medStatus != null) {
                MediaStatus = System.Enum.GetName(typeof(PlayerStateType), medStatus.PlayerState) ?? "";
                CurrentId = medStatus.CurrentItemId;
                FirstTrack = currentFirstTrack;
                LastTrack = currentLastTrack;
            } else {
                MediaStatus = "";
            }
        }

        public double VolumeLevel { get; private set; }

        public int AppCount { get; private set; }
        public string AppID { get; private set; }
        public string AppName { get; private set; }
        public string AppStatus { get; private set; }
        public string MediaStatus { get; private set; }


        public int? CurrentId { get; private set; }
        public bool FirstTrack { get; private set; }
        public bool LastTrack { get; private set; }


        //public ChromecastStatus? Status { get; set; }
        //public MediaStatus? MediaStatus { get; set; }
    }

    public class ChromeCastWrapper : IHostedService, IChromeCastWrapper {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger Log;
        private readonly List<ChromecastReceiver> Receivers = new();
        private readonly String ccName;
        private readonly String appId;

        private ChromecastClient? ConnectedClient = null;
        private QueueMediaChannel? mediaChannel = null;
        private StatusChannel<ReceiverStatusMessage, ChromecastStatus>? rcChannel = null;
        private Double? currentVolume = null;
        private MediaStatus? currentMediaStatus = null;

        private bool currentFirstTrack = false;
        private bool currentLastTrack = false;

        public event EventHandler? StatusChanged;

        //private StatusChannel<MediaStatusMessage, IEnumerable<MediaStatus>>? mediaStatusChannel = null;


        public ChromeCastWrapper(IConfiguration conf, ILoggerFactory logFactory) { 
            this.loggerFactory = logFactory;
            Log = logFactory.CreateLogger<ChromeCastWrapper>();
            Log.LogDebug("Constructor called");

            ccName = conf.GetValue<String>("CcName", "") ?? "";
            appId = conf.GetValue<String>("CcAppId", "") ?? "";

            Log.LogDebug("using cc filter:{name}* and appId {appId}", ccName , appId);
        }

        //private IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();

        public Task StartAsync(CancellationToken cancellationToken) {
            Log.LogDebug("StartAsync called");

            IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
            locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
            _ = locator.FindReceiversAsync(CancellationToken.None);         // Fire the search process and wait for receiver found events!

            //Receivers = (await locator.FindReceiversAsync()).ToList();    // This blocks for 2000ms here!
            //Log.LogDebug("found " + Receivers.Count() + " receivers.");
            return Task.CompletedTask;
        }

        private async void Locator_ChromecastReceivedFound(object? sender, ChromecastReceiver e) {
            Log.LogDebug("found {name}", e.Name);
            Receivers.Add(e);

            if (ConnectedClient == null) {
                if (e.Name.StartsWith(ccName, StringComparison.OrdinalIgnoreCase)) {
                    Log.LogDebug("{name}[{status}] fits the filter -> connect client", e.Name, e.Status);
                    await ConnectNewClient(e);
                }
            }

        }

        private async Task ConnectNewClient(ChromecastReceiver e) {
            ConnectedClient = QueueCaster.ChromecastClient.CreateQueueCasterClient(loggerFactory);

            var st = await ConnectedClient.ConnectChromecast(e);
            Log.LogDebug("Connected available App[0]: {appid}", (((st?.Applications?.Count??0) > 0)? st?.Applications[0].AppId : "<null>"));
            mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
            if (mediaChannel != null) {
                mediaChannel.QueueMediaStatusChanged += MediaChannel_StatusChanged;
            }
            rcChannel = ConnectedClient.GetChannel<StatusChannel<ReceiverStatusMessage, ChromecastStatus>>();
            if (rcChannel != null) {
                //StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = (StatusChannel<ReceiverStatusMessage, ChromecastStatus>)rcChannel;
                rcChannel.StatusChanged += StatusChannel_StatusChanged;
            }
            
            st = await ConnectedClient.LaunchApplicationAsync(appId, true);

            ConnectedClient.Disconnected += ConnectedClient_Disconnected;
            Log.LogDebug("Launched/joined App[0]: {appId}",(((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
        }

        private void ConnectedClient_Disconnected(object? sender, EventArgs e) {
            try {
                Log.LogDebug("Disconnect received -> Trying to gracefully shutdown client and reconnect a new one!");
                if (ConnectedClient != null) {
                    ConnectedClient.Disconnected -= ConnectedClient_Disconnected;
                }
                if (rcChannel != null) {
                    rcChannel.StatusChanged -= StatusChannel_StatusChanged;
                }
                if (mediaChannel != null) {
                    mediaChannel.QueueMediaStatusChanged -= MediaChannel_StatusChanged;
                }
            } finally {
                ConnectedClient = null;
            }

            // We reconnect new application in order to be operable again. 
            var receiver = Receivers.Where(r => r.Name.StartsWith(this.ccName)).ToList().FirstOrDefault();
            if (receiver != null) {
                Log.LogDebug("Reconnecting to CC '{name}'.", receiver.Name);
                _ = ConnectNewClient(receiver);
            }
        }

        private void MediaChannel_StatusChanged(object? sender, EventArgs e) {
            if (e is MediaStatusChangedEventArgs msm) { 
                Log.LogTrace("{cnt} changed event(s):", msm.Status.Count);      // I never got more than one here.
                if (msm.Status.Count > 0) {
                    int idx = 0;
                    foreach (var item in msm.Status) {
                        currentMediaStatus = item;
                        int currentId = item.CurrentItemId;

                        if (currentMediaStatus?.QueueItems != null) {
                            if (currentMediaStatus.QueueItems.Length == 2) {
                                currentFirstTrack = false;
                                currentLastTrack = false;
                                if (currentMediaStatus.QueueItems[0].ItemId == currentId) {
                                    currentFirstTrack = true;
                                } else if (currentMediaStatus.QueueItems[1].ItemId == currentId) {
                                    currentLastTrack = true;
                                } 
                            } else {
                                currentFirstTrack = false;
                                currentLastTrack = false;
                            }
                        }
                        StatusChanged?.Invoke(this, new CCWStatusEventArgs(rcChannel?.Status, item, currentFirstTrack, currentLastTrack));
                        Log.LogDebug("Media-SessionId: {MediaSessionId} {PlayerState} id: {CurrentItemId} at {CurrentTime}",
                                     item.MediaSessionId,
                                     item.PlayerState,
                                     item.CurrentItemId,
                                     item.CurrentTime);
                        idx++;
                    }
                }
            }
        }

        private void StatusChannel_StatusChanged(object? sender, EventArgs e) {

            currentVolume = rcChannel?.Status.Volume.Level;
            StatusChanged?.Invoke(this, new CCWStatusEventArgs(rcChannel?.Status, currentMediaStatus, currentFirstTrack, currentLastTrack));
            Log.LogDebug("StatusChanged Vol: {volume}", currentVolume);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log.LogDebug("CCW - StopAsync called");
            return Task.CompletedTask;
        }

        //public async Task PlayStop() {
        //    if (mediaChannel != null) {
        //        Log?.LogDebug("Stop Media");
        //        currentMediaStatus ??= await mediaChannel.GetStatusAsync();
        //        if (currentMediaStatus != null) {
        //            await mediaChannel.StopAsync(currentMediaStatus.MediaSessionId);
        //        }
        //    }
        //}


        public async Task PlayNext() {
            if (mediaChannel != null) {
                Log?.LogDebug("Play Next");
                currentMediaStatus ??= await mediaChannel.GetStatusAsync();
                if (currentMediaStatus != null) {
                    await mediaChannel.QueueNextAsync(currentMediaStatus.MediaSessionId);
                }
            }
        }

        public async Task PlayPrev() {
            if (mediaChannel != null) {
                Log?.LogDebug("Play Prev");
                currentMediaStatus ??= await mediaChannel.GetStatusAsync();
                if (currentMediaStatus != null) {
                    await mediaChannel.QueuePrevAsync(currentMediaStatus.MediaSessionId);
                }
            }
        }

        public async Task VolumeDown() {
            if (rcChannel != null) {
                currentVolume = (currentVolume ?? 0.2) - 0.03;
                if (currentVolume < 0) {
                    currentVolume = 0;
                }
                Log?.LogDebug("Vol- [{vol}]", String.Format("{0:0.000}", currentVolume));
                await ((IReceiverChannel)rcChannel).SetVolume(currentVolume ?? 0.1);
            }
        }

        public async Task VolumeUp() {
            if (rcChannel != null) {
                currentVolume = (currentVolume ?? 0.1) + 0.03;
                if (currentVolume > 0.6) {
                    currentVolume = 0.6;
                }
                Log?.LogDebug("Vol+ [{vol}]", String.Format("{0:0.000}", currentVolume));
                await ((IReceiverChannel)rcChannel).SetVolume(currentVolume ?? 0.1);
            }
        }

        public async Task<MediaStatus?> PlayCdTracks(List<(string url, string name)> tracks) {
            MediaStatus? status = null;
            if (mediaChannel != null) {
                QueueItem[]? qi = new QueueItem[tracks.Count];
                int i = 0;
                foreach (var (url, name) in tracks) {
                    var media = new Media {
                        ContentUrl = url,
                        StreamType = StreamType.Buffered,
                        ContentType = "audio/mp4",
                        Metadata = new MediaMetadata() { Title = name }
                    };
                    qi[i] = new QueueItem() { Media = media, OrderId = i, StartTime = 0 };
                    i++;
                }
                Log.LogDebug("Queueing {trackCnt} tracks.", qi.Length);
                status = await mediaChannel.QueueLoadAsync(qi).ConfigureAwait(false);
                StatusChanged?.Invoke(this, new CCWStatusEventArgs(rcChannel?.Status, status, currentFirstTrack, currentLastTrack));
            }
            return status;
        }

        public async Task<MediaStatus?> PlayLive(string url, string? name = null) {
            MediaStatus? status = null;
            if (mediaChannel != null) {
                var media = new Media {
                    ContentUrl = url,
                    StreamType = StreamType.Live,
                    ContentType = "audio/mp4",
                    Metadata = new MediaMetadata() { Title = name ?? url }
                };
                Log.LogDebug("Load Media.");
                status = await mediaChannel.LoadAsync(media);
            }
            return status;
        }

        public async Task Shutdown() {
            if (ConnectedClient != null) {
                await ConnectedClient.DisconnectAsync();
            }
        }
    }
}
