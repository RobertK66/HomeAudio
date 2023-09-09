using ABI.System;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
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
    public class ChromeCastClient : INotifyPropertyChanged {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        DispatcherQueue _dispatcherQueue;
        QueueCaster.ChromecastClient ConnectedClient = null;

        private ChromecastReceiver cr;
        private String _name;
        private String _status;
        private String _mediaStatus;
        private String _appId;

        private int _volume;

        public ChromeCastClient(ChromecastReceiver cr, DispatcherQueue dc) {
            this.cr = cr;
            Name = cr.Name;
            Status = cr.Status;
            _dispatcherQueue = dc;
        }

        public String Name { get { return _name; } set { _name = value; RaisePropertyChanged(); } }
        public String Status { get { return _status; } set { _status = value; RaisePropertyChanged(); } }
        public int Volume { get { return _volume; } set { _volume = value; RaisePropertyChanged(); } }
        public String MediaStatus { get { return _mediaStatus; } set { _mediaStatus = value; RaisePropertyChanged(); } }

        public String AppId { get { return _appId; } set { _appId = value; RaisePropertyChanged(); } }


        internal async Task TryConnectAsync() {
            ConnectedClient = QueueCaster.ChromecastClient.CreateQueueCasterClient(null);

            var st = await ConnectedClient.ConnectChromecast(cr);
            //Log.LogDebug("Connected available App[0]: {appid}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            var mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
            if (mediaChannel != null) {
                mediaChannel.QueueMediaStatusChanged += MediaChannel_QueueMediaStatusChanged; ;
                var rcChannel = ConnectedClient.GetChannel<StatusChannel<ReceiverStatusMessage, ChromecastStatus>>();
                if (rcChannel != null) {
                    rcChannel.StatusChanged += RcChannel_StatusChanged;
                }

                st = await ConnectedClient.LaunchApplicationAsync("9B5A75B4", true);    // TODO: APPID from config!

                ConnectedClient.Disconnected += ConnectedClient_Disconnected;
                //Log.LogDebug("Launched/joined App[0]: {appId}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            }
            
        }

        private void ConnectedClient_Disconnected(object sender, EventArgs e) {
            //throw new NotImplementedException();
        }

        private void RcChannel_StatusChanged(object sender, EventArgs e) {
            //throw new NotImplementedException();
            StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = sender as StatusChannel<ReceiverStatusMessage, ChromecastStatus>;
            if (sc != null) {
                //Log.LogTrace("Status changed: " + sc.Status.Volume.Level.ToString());

                _dispatcherQueue.TryEnqueue(() => {
                    Volume = (int)(sc.Status.Volume.Level * 200);
                    Status = sc.Status.Applications.FirstOrDefault()?.StatusText;
                    AppId = sc.Status.Applications.FirstOrDefault()?.AppId +  "/" + sc.Status.Applications.FirstOrDefault()?.DisplayName;
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
    }
}
