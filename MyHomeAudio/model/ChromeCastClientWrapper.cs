using ABI.System;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Receiver;
using Sharpcaster.Models;
using Sharpcaster.Models.ChromecastStatus;
using Sharpcaster.Models.Media;
using Sharpcaster.Models.Protobuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Radios;

namespace MyHomeAudio.model {
    public class ChromeCastClientWrapper : INotifyPropertyChanged {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        DispatcherQueue _dispatcherQueue;
        String _connectionAppId;
        ChromecastClient ConnectedClient = null;


        private ChromecastReceiver cr;
        private String _name;
        private String _status;
        private String _mediaStatus;
        private String _appId;
        private bool _isConnected;
        private ILoggerFactory _loggerFactory;

        private int _volume;

        public ChromeCastClientWrapper(ChromecastReceiver cr, DispatcherQueue dc, ILoggerFactory lf) {
            this.cr = cr;
            Name = cr.Name;
            Status = cr.Status;
            _dispatcherQueue = dc;
            _loggerFactory = lf;
        }

        public String Name { get { return _name; } set { _name = value; RaisePropertyChanged(); } }
        public String Status { get { return _status; } set { _status = value; RaisePropertyChanged(); } }
        public int Volume { get { return _volume; } set { _volume = value; RaisePropertyChanged(); } }
        public String MediaStatus { get { return _mediaStatus; } set { _mediaStatus = value; RaisePropertyChanged(); } }
        public String AppId { get { return _appId; } set { _appId = value; RaisePropertyChanged(); } }
        public bool IsConnected { get { return _isConnected; } set { _isConnected = value; RaisePropertyChanged(); } }

        public async Task<bool> TryConnectAsync(string appId) {
            //bool connected = false;
            IsConnected = false;
            _connectionAppId = appId;

            ConnectedClient = QueueCaster.ChromecastClient.CreateQueueCasterClient(_loggerFactory);

            //cr.DeviceUri = new System.Uri("http://localhost:123/base");
            var st = await ConnectedClient.ConnectChromecast(cr);
            //Log.LogDebug("Connected available App[0]: {appid}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            var mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
            if (mediaChannel != null) {
                mediaChannel.QueueMediaStatusChanged += MediaChannel_QueueMediaStatusChanged; ;
                var rcChannel = ConnectedClient.GetChannel<StatusChannel<ReceiverStatusMessage, ChromecastStatus>>();
                if (rcChannel != null) {
                    rcChannel.StatusChanged += RcChannel_StatusChanged;
                }
                st = await ConnectedClient.LaunchApplicationAsync(appId, true);   
                
                ConnectedClient.Disconnected += ConnectedClient_Disconnected;
                IsConnected = true;
                //Log.LogDebug("Launched/joined App[0]: {appId}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            }
            return IsConnected;
        }

        private void ConnectedClient_Disconnected(object sender, EventArgs e) {
            // This client is done now -> reconnect a new one.
            _dispatcherQueue.TryEnqueue(async () => {
                IsConnected = false;
                ConnectedClient = null;
                await Task.Delay(3000);
                _ = TryConnectAsync(_connectionAppId);
            });
        }

        private void RcChannel_StatusChanged(object sender, EventArgs e) {
            //throw new NotImplementedException();
            StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = sender as StatusChannel<ReceiverStatusMessage, ChromecastStatus>;
            if (sc != null) {
                //Log.LogTrace("Status changed: " + sc.Status.Volume.Level.ToString());

                _dispatcherQueue.TryEnqueue(() => {
                    if (sc.Status?.Volume != null) {
                        Volume = (int)(sc.Status?.Volume?.Level * 200);
                    }
                    Status = sc.Status?.Applications?.FirstOrDefault()?.StatusText;
                    AppId = sc.Status?.Applications?.FirstOrDefault()?.AppId +  "/" + sc.Status?.Applications?.FirstOrDefault()?.DisplayName;
                });


            }
        }

        private void MediaChannel_QueueMediaStatusChanged(object sender, MediaStatusChangedEventArgs e) {
            QueueMediaChannel mc = sender as QueueMediaChannel;
            if (mc != null) {
                _dispatcherQueue.TryEnqueue(() => {
                    MediaStatus = e.Status.FirstOrDefault()?.PlayerState.ToString();
                });

                //Log.LogTrace("MediaChanel Status changed: " + e.Status.FirstOrDefault()?.CurrentTime.ToString() ?? "<->");
            }
        }

        internal async Task PlayRadioAsync(NamedUrl url) {
            await semaphoreSlim.WaitAsync();    // Only one Play at once is routet to LoadAsync!
            try {
                if (ConnectedClient != null) { 
                            var mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
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
                    var mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
                    if (mediaChannel != null) {
                        List<QueueItem> media = new List<QueueItem>();
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
            } finally {
                semaphoreSlim?.Release();
            }
        }

        internal void VolumeUp() {
            var rcChannel =  ConnectedClient?.GetChannel<ReceiverChannel>();
            if (rcChannel != null) {
                Volume = (Volume) + 3;
                if (Volume > 100) {
                    Volume = 100;
                }
                //Log?.LogDebug("Vol- [{vol}]", String.Format("{0:0.000}", Volume));
                _ = rcChannel.SetVolume(((double)Volume)/200);
            }
        }

        internal void VolumeDown() {
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
    }
}
