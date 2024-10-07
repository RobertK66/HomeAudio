using AudioCollectionApi.api;
using AudioCollectionApi.model;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Sharpcaster;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Models;
using Sharpcaster.Models.Media;
using Sharpcaster.Models.Protobuf;
using Sharpcaster.Models.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaloniaHomeAudio.player {
    public partial class ChromeCastPlayer : ObservableObject, IChromeCastPlayer {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private CancellationTokenSource cancelLocator;

        private ChromecastReceiver _receiver;
        private ChromecastClient? chromecastClient = null;
        MediaChannel? mediaChannel = null;
        ReceiverChannel? rcChannel = null;
        private bool IsConnected = false;

        [ObservableProperty]
        string _playerStatus = "new";

        public ChromeCastPlayer(ILoggerFactory lf, IChromecastLocator locator) {
            _loggerFactory = lf;
            _logger = lf.CreateLogger<ChromeCastPlayer>();
            PlayerStatus = "Searching Receiver...";
            locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
            cancelLocator = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
            //_ = locator.FindReceiversAsync(cancelLocator.Token);
        }

        private bool IsFittingReceiver(ChromecastReceiver cr) {
            return cr.Name.StartsWith("My JBL");
        }

        private async Task DisconnectLocalClientAsync() {
            if (mediaChannel != null) {
                mediaChannel.StatusChanged -= MediaChannel_StatusChanged;
                mediaChannel = null;
            }
            if (rcChannel != null) {
                rcChannel.StatusChanged -= RcChannel_StatusChanged;
                rcChannel = null;
            }
            if (chromecastClient != null) {
                chromecastClient.Disconnected -= RcChannel_StatusChanged;
                await chromecastClient.DisconnectAsync();
            }
        }


        private async Task TryConnectAsync(ChromecastReceiver cr, string appId) {
            IsConnected = false;
            PlayerStatus = "Connecting...";
            try {
                _ = DisconnectLocalClientAsync();
                chromecastClient = new ChromecastClient(_loggerFactory);

                var st = await chromecastClient.ConnectChromecast(cr);

                string? oldAppid = null;
                if ((st?.Applications?.Count ?? 0) > 0) {
                    oldAppid = st?.Applications[0].AppId;
                }
                _logger.LogDebug("Connected - available App[0]: {appid}", oldAppid);

                mediaChannel = chromecastClient.GetChannel<MediaChannel>();
                if (mediaChannel != null) {
                    mediaChannel.StatusChanged += MediaChannel_StatusChanged;
                    rcChannel = chromecastClient.GetChannel<ReceiverChannel>();
                    if (rcChannel != null) {
                        rcChannel.StatusChanged += RcChannel_StatusChanged;
                    }
                    st = await chromecastClient.LaunchApplicationAsync(appId, true);
                    _logger.LogDebug("************Launched/Joined App[0]: {appid}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));

                    chromecastClient.Disconnected += ChromecastClient_Disconnected;
                    IsConnected = true;
                    PlayerStatus = "Connected";
                }
            } catch (Exception ex) {
                _logger.LogError("Exception while trying to connect chromecast: {ex}", ex);
                _ = DisconnectLocalClientAsync();
                chromecastClient = null;
            }
        }

        private void ChromecastClient_Disconnected(object? sender, EventArgs e) {
            PlayerStatus = "Disconnecting";
            IsConnected = false;
            _ = DisconnectLocalClientAsync();
        }

        private void RcChannel_StatusChanged(object? sender, EventArgs e) {
            if (sender is ReceiverChannel sc) {
                //  .LogDebug("Status changed: " + sc.Status.Volume.Level.ToString());

                if (sc.Status?.Volume?.Level != null) {
                    try {
                        PlayerStatus = "Vol: " + (sc.Status.Volume.Level * 200).ToString();
                    } catch (Exception) {

                    }
                }

                    //_dispatcherQueue.TryEnqueue(() => {
                    //    if (sc.Status?.Volume?.Level != null) {
                    //        Volume = (int)(sc.Status.Volume.Level * 200);
                    //    }
                    //    Status = sc.Status?.Applications?.FirstOrDefault()?.StatusText ?? "<no status>";
                    //    AppId = sc.Status?.Applications?.FirstOrDefault()?.AppId + "/" + sc.Status?.Applications?.FirstOrDefault()?.DisplayName;
               


            }
        }

        private void MediaChannel_StatusChanged(object? sender, EventArgs e) {
            //throw new NotImplementedException();
        }

        private void Locator_ChromecastReceivedFound(object? sender, Sharpcaster.Models.ChromecastReceiver e) {
            if (IsFittingReceiver(e)) {
                cancelLocator.Cancel();
                PlayerStatus = "Found Receiver";
                _receiver = e;
                // make and connect new Client
                _ = TryConnectAsync(_receiver, "46C1A819");
            }
        }

        public void Play(IMedia media) {
            _logger.LogInformation("Play: "+ media.Name);
            if (!IsConnected) {
                PlayerStatus = "Reconnecting";
                _ = TryConnectAsync(_receiver, "46C1A819");
                
            } else { 
            // ((IsConnected) && (mediaChannel != null)) {
                if (media is Cd cd) {
                    var items = new List<QueueItem>();
                    foreach (var t in cd.Tracks) {
                        var qi = new QueueItem() {
                            Media = new Media {
                                ContentUrl = t.ContentUrl,
                                StreamType = StreamType.Buffered,
                                ContentType = "audio/mp4",
                                Metadata = new MediaMetadata() { Title = t.Name ?? t.ContentUrl }
                            }
                        };
                        items.Add(qi);
                    }
                    _ = mediaChannel?.QueueLoadAsync(items.ToArray());
                } else if (media is NamedUrl radio) {
                    var item = new Media {
                        ContentUrl = radio.ContentUrl,
                        StreamType = StreamType.Live,
                        ContentType = "audio/mp4",
                        Metadata = new MediaMetadata() { Title = radio.Name }
                    };
                    _ = mediaChannel?.LoadAsync(item);
                }
            } 
        }

        public void VolumeDown() {
            rcChannel?.SetVolume(0.1);
        }

        public void VolumeUp() {
            rcChannel?.SetVolume(0.3);
        }
    }
}
