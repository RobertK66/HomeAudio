using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
//using Sharpcaster;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Receiver;
using Sharpcaster.Models.ChromecastStatus;
using Sharpcaster.Models.Media;
//using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStatus = QueueCaster.MediaStatus;

namespace ConGui {
    public class CCStatusEventArgs : EventArgs {
        public CCStatusEventArgs(ChromecastStatus status) {
            Status = status;
        }

        public ChromecastStatus Status { get; set; }
    }

    public class CCStarter : IHostedService {

        private static ILoggerFactory? LoggerFactory;
        private static ILogger? Log;

        private String ccName;
        private String appId;
        private QueueMediaChannel? mediaChannel;
        private ReceiverChannel? rcChannel;
        private Double? currentVolume = null;

        public event EventHandler StatusChanged;

        public CCStarter(IConfiguration conf, ILoggerFactory loggerFac) {
            ccName = conf.GetValue<String>("CcName", "") ?? "";
            appId = conf.GetValue<String>("CcAppId", "") ?? "";
            LoggerFactory = loggerFac; 
            Log = loggerFac.CreateLogger<CCStarter>();
        }

        //public CCStarter(string ccName, string appId) {
        //    this.ccName = ccName;
        //    this.appId = appId;
        //}

        public async Task Connect() {
            IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
            var chromecasts = await locator.FindReceiversAsync();

            Log?.LogDebug("CC Cnt:" + chromecasts.Count());
            var cc = chromecasts.Where(c => c.Name.StartsWith(ccName)).FirstOrDefault();
            if (cc != null) {
                Log?.LogDebug("**** Status: " + cc.Status);

                var client = QueueCaster.ChromecastClient.CreateQueueCasterClient(LoggerFactory);
                var st = await client.ConnectChromecast(cc);
                mediaChannel = client.GetChannel<QueueMediaChannel>();
                rcChannel = client.GetChannel<ReceiverChannel>();
                if (rcChannel != null) {
                    StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = (StatusChannel<ReceiverStatusMessage, ChromecastStatus>)rcChannel;
                    sc.StatusChanged += RcChannel_StatusChanged;
                }
                st = await client.LaunchApplicationAsync(appId, true);
            }
        }

        private void RcChannel_StatusChanged(object? sender, EventArgs e) {
            StatusChannel<ReceiverStatusMessage, ChromecastStatus>? sc = sender as StatusChannel<ReceiverStatusMessage, ChromecastStatus>;
            if (sc != null) {
                currentVolume = sc.Status.Volume.Level;
                StatusChanged?.Invoke(this, new CCStatusEventArgs(sc.Status));
            }
        }

        public async Task VolumeUp() {
            if (rcChannel != null) {
                currentVolume = (currentVolume ?? 0.1) + 0.03;
                if (currentVolume > 0.6) {
                    currentVolume = 0.6;
                }
                Log?.LogDebug("Vol+ [" + String.Format("{0:0.000}", currentVolume) + "]");
                var stat = await ((IReceiverChannel)rcChannel).SetVolume(currentVolume??0.1);
                currentVolume = stat.Volume.Level;
            }
        }
        public async Task VolumeDown() {
            if (rcChannel != null) {
                currentVolume = (currentVolume ?? 0.2) - 0.03;
                if (currentVolume < 0) {
                    currentVolume = 0;
                }
                Log?.LogDebug("Vol- [" + String.Format("{0:0.000}", currentVolume) + "]");
                var stat = await ((IReceiverChannel)rcChannel).SetVolume(currentVolume??0.1);
                currentVolume = stat.Volume.Level;
            }
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
                status = await mediaChannel.LoadAsync(media);
            }
            return status;
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


        public async Task<MediaStatus?> PlayNext() {
            if (mediaChannel != null) {
                var st1 = await mediaChannel.GetStatusAsync();
                if (st1 != null) {
                    return await mediaChannel.QueueNextAsync(st1.MediaSessionId);
                }
            }
            return null;
        }

        public async Task<MediaStatus?> PlayPrev() {
            if (mediaChannel != null) {
                var st1 = await mediaChannel.GetStatusAsync();
                if (st1 != null) {
                    return await mediaChannel.QueuePrevAsync(st1.MediaSessionId);
                }
            }
            return null;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            Log?.LogDebug("SartAsync called");
            await Connect();
            Log?.LogDebug("SartAsync finished");
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log?.LogDebug("StopAsync called");
            return Task.CompletedTask;
        }
    }
}
