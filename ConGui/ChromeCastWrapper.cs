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
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using MediaStatus = QueueCaster.MediaStatus;

namespace ConGui {
    public class CCWStatusEventArgs : EventArgs {
        public CCWStatusEventArgs(ChromecastStatus status) {
            Status = status;
        }

        public ChromecastStatus Status { get; set; }
    }


    public class ChromeCastWrapper : IHostedService, IChromeCastWrapper {
        private ILoggerFactory loggerFactory;
        private ILogger Log;
        private List<ChromecastReceiver> Receivers = new List<ChromecastReceiver>();
        private readonly String ccName;
        private readonly String appId;

        private ChromecastClient? ConnectedClient = null;
        private QueueMediaChannel? mediaChannel = null;
        private StatusChannel<ReceiverStatusMessage, ChromecastStatus>? rcChannel = null;
        private Double? currentVolume = null;

        public event EventHandler? StatusChanged;

        private StatusChannel<MediaStatusMessage, IEnumerable<MediaStatus>>? mediaStatusChannel = null;


        public ChromeCastWrapper(IConfiguration conf, ILoggerFactory logFactory) { 
            this.loggerFactory = logFactory;
            Log = logFactory.CreateLogger<ChromeCastWrapper>();
            Log.LogDebug("Constructor called");

            ccName = conf.GetValue<String>("CcName", "") ?? "";
            appId = conf.GetValue<String>("CcAppId", "") ?? "";

            Log.LogDebug("using " + ccName + "* " + appId);
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            Log.LogDebug("StartAsync called");

            IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
            locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
            _ = locator.FindReceiversAsync();       // Fire the search process and wait for receiver found events!

            //Receivers = (await locator.FindReceiversAsync()).ToList();    // This blocks for 2000ms here!
            //Log.LogDebug("found " + Receivers.Count() + " receivers.");
            return Task.CompletedTask;
        }

        private async void Locator_ChromecastReceivedFound(object? sender, ChromecastReceiver e) {
            Log.LogDebug("found " + e.Name);
            Receivers.Add(e);

            if (ConnectedClient == null) {
                if (e.Name.StartsWith(ccName, StringComparison.OrdinalIgnoreCase)) {
                    Log.LogDebug(e.Name + "["+e.Status + "] fits the filter -> connect client");
                    ConnectedClient = QueueCaster.ChromecastClient.CreateQueueCasterClient(loggerFactory);

                    var st = await ConnectedClient.ConnectChromecast(e);
                    Log.LogDebug(st.ToString());
                    mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
                    if (mediaChannel != null) {
                        mediaChannel.StatusChanged += MediaChannel_StatusChanged;
                    }
                    rcChannel = ConnectedClient.GetChannel<StatusChannel<ReceiverStatusMessage, ChromecastStatus>>();
                    if (rcChannel != null) {
                        //StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = (StatusChannel<ReceiverStatusMessage, ChromecastStatus>)rcChannel;
                        rcChannel.StatusChanged += StatusChannel_StatusChanged; 
                    }
                    st = await ConnectedClient.LaunchApplicationAsync(appId, true);
                    Log.LogDebug(st.ToString());
                }
            }

        }

        private void MediaChannel_StatusChanged(object? sender, EventArgs e) {
            Log.LogDebug(((QueueMediaChannel?)sender)?.Status.ToString());
        }

        private void StatusChannel_StatusChanged(object? sender, EventArgs e) {
            //Log.LogDebug(e.ToString());
            currentVolume = rcChannel?.Status.Volume.Level;
            StatusChanged?.Invoke(this, new CCWStatusEventArgs(rcChannel?.Status??new ChromecastStatus()));
    }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log.LogDebug("CCW - StopAsync called");
            return Task.CompletedTask;
        }

        public async Task PlayNext() {
            if (mediaChannel != null) {
                var st1 = await mediaChannel.GetStatusAsync();
                if (st1 != null) {
                    _ = mediaChannel.QueueNextAsync(st1.MediaSessionId);
                }
            }
        }

        public async Task PlayPrev() {
            if (mediaChannel != null) {
                var st1 = await mediaChannel.GetStatusAsync();
                if (st1 != null) {
                    _ = mediaChannel.QueuePrevAsync(st1.MediaSessionId);
                }
            }
        }

        public void VolumeDown() {
            if (rcChannel != null) {
                currentVolume = (currentVolume ?? 0.2) - 0.03;
                if (currentVolume < 0) {
                    currentVolume = 0;
                }
                Log?.LogDebug("Vol- [{vol}]", String.Format("{0:0.000}", currentVolume));
                _ = ((IReceiverChannel)rcChannel).SetVolume(currentVolume ?? 0.1);
                //currentVolume = stat.Volume.Level;
            }
        }

        public void VolumeUp() {
            if (rcChannel != null) {
                currentVolume = (currentVolume ?? 0.1) + 0.03;
                if (currentVolume > 0.6) {
                    currentVolume = 0.6;
                }
                Log?.LogDebug("Vol+ [{vol}]", String.Format("{0:0.000}", currentVolume));
                _ = ((IReceiverChannel)rcChannel).SetVolume(currentVolume ?? 0.1);
                //currentVolume = stat.Volume.Level;
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
                status = await mediaChannel.QueueLoadAsync(qi).ConfigureAwait(false);
            }
            return status;
        }

        public  async Task<MediaStatus?> PlayLive(string url, string? name = null) {
            MediaStatus? status = null;
            if (mediaChannel != null) {
                var media = new Media {
                    ContentUrl = url,
                    StreamType = StreamType.Live,
                    ContentType = "audio/mp4",
                    Metadata = new MediaMetadata() { Title = name ?? url }
                };
                status = await mediaChannel.LoadAsync(media);
            }
            return status;
        }
    }
}
