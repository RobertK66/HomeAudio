using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Sharpcaster;
using Sharpcaster.Channels;
using Sharpcaster.Models;
using Sharpcaster.Models.Media;
using Sharpcaster.Models.Queue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WinUiHomeAudio.model {
    public class ChromeCastClientWrapper : INotifyPropertyChanged, IPlayerProxy {
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly DispatcherQueue _dispatcherQueue;
        private String _connectionAppId = "";
        private ChromecastClient? ConnectedClient = null;


        private ChromecastReceiver cr;
        private String _name;
        private String _status;
        private String? _mediaStatus;
        private String? _appId;
        private bool _isConnected;
        private bool _isOn;

        private MediaStatus? currentMediaStatus = null;

        private ILoggerFactory _loggerFactory;

        private ILogger Log;

        private int _volume;

        public ChromeCastClientWrapper(ChromecastReceiver cr, DispatcherQueue dc, ILoggerFactory lf) {
            this.cr = cr;
            _name = cr.Name;
            _status = cr.Status;
            _dispatcherQueue = dc;
            _loggerFactory = lf;
            _isOn = false;
            Log = lf.CreateLogger<ChromeCastClientWrapper>();
        }

        public String Name { get { return _name; } set { _name = value; RaisePropertyChanged(); } }
        public String Status { get { return _status; } set { _status = value; RaisePropertyChanged(); } }
        public int Volume { get { return _volume; } set { _volume = value; RaisePropertyChanged(); } }
        public String? MediaStatus { get { return _mediaStatus; } set { _mediaStatus = value; RaisePropertyChanged(); } }
        public String? AppId { get { return _appId; } set { _appId = value; RaisePropertyChanged(); } }
        public bool IsConnected { get { return _isConnected; } set { _isConnected = value; RaisePropertyChanged(); } }

        public bool IsOn { get { return _isOn; } set { _isOn = value; RaisePropertyChanged(); } }

        public string Id => "none";

        public async Task<bool> TryConnectAsync(string appId) {
            //bool connected = false;
            IsConnected = false;
            MediaChannel? mediaChannel = null;
            ReceiverChannel? rcChannel = null;
            try {
                _connectionAppId = appId;

                ConnectedClient = new ChromecastClient(loggerFactory: _loggerFactory);
                var st = await ConnectedClient.ConnectChromecast(cr);

                string? oldAppid = null;
                if ((st?.Applications?.Count ?? 0) > 0) {
                    oldAppid = st?.Applications[0].AppId;
                }
                Log.LogDebug("Connected - available App[0]: {appid}", oldAppid);

                mediaChannel = ConnectedClient.GetChannel<MediaChannel>();
                if (mediaChannel != null) {
                    mediaChannel.StatusChanged += MediaChannel_QueueMediaStatusChanged; ;
                    rcChannel = ConnectedClient.GetChannel<ReceiverChannel>();
                    if (rcChannel != null) {
                        rcChannel.StatusChanged += RcChannel_StatusChanged;
                    }
                    st = await ConnectedClient.LaunchApplicationAsync(appId, true);
                    Log.LogDebug("************Launched/Joined App[0]: {appid}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));

                    ConnectedClient.Disconnected += ConnectedClient_Disconnected;
                    IsConnected = true;
                    IsOn = true;
                }
            } catch (Exception ex) {
                Log.LogError("Exception while trying to connect chromecast: {ex}", ex);
                if (mediaChannel != null) {
                    mediaChannel.StatusChanged -= MediaChannel_QueueMediaStatusChanged;
                }
                if (rcChannel != null) {
                    rcChannel.StatusChanged -= RcChannel_StatusChanged;
                }
                if (ConnectedClient != null) {
                    ConnectedClient.Disconnected -= ConnectedClient_Disconnected;
                    _ = ConnectedClient.DisconnectAsync();
                    ConnectedClient = null;
                }
            }
            return IsConnected;
        }

        private void ConnectedClient_Disconnected(object? sender, EventArgs e) {
            // This client is done now -> reconnect a new one.
            _dispatcherQueue.TryEnqueue(async () => {
                IsConnected = false;
                IsOn = false;
                ConnectedClient = null;
                await Task.Delay(3000);
                //_ = TryConnectAsync(_connectionAppId);
            });
        }




        private void RcChannel_StatusChanged(object? sender, EventArgs e) {
            //throw new NotImplementedException();
            if (sender is ReceiverChannel sc) {
                //Log.LogTrace("Status changed: " + sc.Status.Volume.Level.ToString());

                _dispatcherQueue.TryEnqueue(() => {
                    if (sc.Status?.Volume?.Level != null) {
                        Volume = (int)(sc.Status.Volume.Level * 200);
                    }
                    Status = sc.Status?.Applications?.FirstOrDefault()?.StatusText ?? "<no status>";
                    AppId = sc.Status?.Applications?.FirstOrDefault()?.AppId + "/" + sc.Status?.Applications?.FirstOrDefault()?.DisplayName;
                });


            }
        }

        private void MediaChannel_QueueMediaStatusChanged(object? sender, EventArgs e) {
            if (sender is MediaChannel mc) {
                _dispatcherQueue.TryEnqueue(() => {
                    MediaStatus = mc.Status.FirstOrDefault()?.PlayerState.ToString() ?? "<leer>";
                });

                //Log.LogTrace("MediaChanel Status changed: " + e.Status.FirstOrDefault()?.CurrentTime.ToString() ?? "<->");
            }
        }

        internal async Task PlayRadioAsync(NamedUrl url) {
            await semaphoreSlim.WaitAsync();    // Only one Play at once is routet to LoadAsync!
            try {
                if (ConnectedClient != null) {
                    var mediaChannel = ConnectedClient.GetChannel<MediaChannel>();
                    if (mediaChannel != null) {
                        var media = new Media {
                            ContentUrl = url.ContentUrl,
                            StreamType = StreamType.Live,
                            ContentType = "audio/mp4",
                            Metadata = new MediaMetadata() { Title = url.Name ?? url.ContentUrl }
                        };
                        //Log.LogDebug("Load Media.");
                        await mediaChannel.LoadAsync(media);
                    }
                }
            } finally {
                semaphoreSlim?.Release();
            }
        }

        internal async Task PlayCdAsync(Cd cd) {
            await semaphoreSlim.WaitAsync();    // Only one Play at once is routet to LoadAsync!
            try {
                if (ConnectedClient != null) {
                    var mediaChannel = ConnectedClient.GetChannel<MediaChannel>();
                    if (mediaChannel != null) {
                        var media = new List<QueueItem>();
                        foreach (var t in cd.Tracks) {
                            var qi = new QueueItem() {
                                Media = new Media {
                                    ContentUrl = t.ContentUrl,
                                    StreamType = StreamType.Buffered,
                                    ContentType = "audio/mp4",
                                    Metadata = new MediaMetadata() { Title = t.Name ?? t.ContentUrl }
                                }
                            };
                            media.Add(qi);
                        }
                        //Log.LogDebug("Load Cd with " + media.Count + " tracks.");
                        await mediaChannel.QueueLoadAsync(media.ToArray());
                    }
                }
            } catch (Exception e) {
                Log.LogError("Exception when loading media: " + e.Message);
            } finally {
                semaphoreSlim?.Release();
            }
        }

        public void VolumeUp() {
            var rcChannel = ConnectedClient?.GetChannel<ReceiverChannel>();
            if (rcChannel != null) {
                Volume = (Volume) + 3;
                if (Volume > 100) {
                    Volume = 100;
                }
                //Log?.LogDebug("Vol- [{vol}]", String.Format("{0:0.000}", Volume));
                _ = rcChannel.SetVolume(((double)Volume) / 200);
            }
        }

        public void VolumeDown() {
            var rcChannel = ConnectedClient?.GetChannel<ReceiverChannel>();
            if (rcChannel != null) {
                Volume = (Volume) - 3;
                if (Volume < 0) {
                    Volume = 0;
                }
                //Log?.LogDebug("Vol- [{vol}]", String.Format("{0:0.000}", Volume));
                _ = rcChannel.SetVolume(((double)Volume) / 200);
            }
        }

        public void Disconnect() {
            if (ConnectedClient != null) {
                IsConnected = false;
                IsOn = false;
                _ = ConnectedClient.DisconnectAsync();
            }
        }




        public async Task StopMediaPlayAsync() {
            await semaphoreSlim.WaitAsync();    // Only one Play at once is routet to LoadAsync!
            try {
                if (ConnectedClient != null) {
                    var mediaChannel = ConnectedClient.GetChannel<MediaChannel>();
                    currentMediaStatus = mediaChannel.Status.FirstOrDefault();

                    if (currentMediaStatus != null) {
                        //Log.LogDebug("Load Media.");
                        await mediaChannel.PauseAsync();
                    }

                }
            } finally {
                semaphoreSlim?.Release();
            }
        }

        public void PlayCd(IMedia cd) {
            if (cd is Cd c) {
                _ = PlayCdAsync(c);
            }
        }

        public void PlayRadio(IMedia radio) {
            if (radio is NamedUrl u) {
                _ = PlayRadioAsync(u);
            }
        }

        public void Stop() {
            _ = StopMediaPlayAsync();
        }

        public void Play() {
            if (ConnectedClient != null) {
                var mediaChannel = ConnectedClient.GetChannel<MediaChannel>();
                currentMediaStatus = mediaChannel.Status.FirstOrDefault();

                if (currentMediaStatus != null) {
                    //Log.LogDebug("Load Media.");
                    _ = mediaChannel.PlayAsync();
                }

            }
        }
    }
}
