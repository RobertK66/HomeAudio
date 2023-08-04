using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Media;
using Sharpcaster.Messages.Receiver;
using Sharpcaster.Models;
using Sharpcaster.Models.ChromecastStatus;
using Sharpcaster.Models.Media;
using Sharpcaster.Models.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Devices.Radios;
using Windows.System;
using Windows.UI.Core;
using Windows.Web.UI;
using WinGuiPackaged.logger;
using WinGuiPackaged.model;
using ChromecastClient = QueueCaster.ChromecastClient;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace WinGuiPackaged {

    public class MainViewModel : IRadioViewModel, ICdViewModel, IPlayerViewModel, INotifyPropertyChanged {

        public ILoggerFactory LoggerFactory { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private logger.LoggerVm _loggerVM;
        public logger.LoggerVm LogWindowViewModel {
            get { return _loggerVM; }
            set {
                _loggerVM = value;
            }
        }

        private ILogger Log;

        private ObservableCollection<Cd> cds = new();
        public ObservableCollection<Cd> Cds { get { return cds; } }
        public Cd SelectedCd { get; set; } = null;


        private ObservableCollection<NamedUrl> radios = new();
        public ObservableCollection<NamedUrl> WebRadios { get { return radios; } }
        public NamedUrl SelectedRadio { get; set; } = null;


        private ObservableCollection<ChromecastReceiver> receiver = new();
        public ObservableCollection<ChromecastReceiver> Receiver { get { return receiver; } }

        private ChromecastReceiver _selectedReceiver = null;
        public ChromecastReceiver SelectedReceiver {
            get { return _selectedReceiver; }
            set {
                if (value != this.SelectedReceiver) {
                    _selectedReceiver = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private int volume = 49;
        public int Volume {
            get { return volume; }
            set {
                if (value != this.Volume) {
                    volume = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Dictionary<String, ChromecastClient> CastClients = new Dictionary<String, ChromecastClient>();

        private DispatcherQueue dispatcherQueue { get; }

        public MainViewModel(ILoggerFactory loggerFactory, logger.LoggerVm logVm) {
            try {
                LoggerFactory = loggerFactory;
                Log = LoggerFactory.CreateLogger<MainViewModel>();
                LogWindowViewModel = logVm;

                dispatcherQueue = logVm.dq;

                receiver.CollectionChanged += Receiver_CollectionChanged;

                var path = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["radioRepos"];
                if (File.Exists(path)) {
                    var cont = JsonSerializer.Deserialize<List<NamedUrl>>(File.ReadAllText(path));
                    foreach (var item in cont) {
                        radios.Add(item);
                    }
                }
                path = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["cdRepos"];
                if (File.Exists(path)) {
                    var cont = JsonSerializer.Deserialize<List<Cd>>(File.ReadAllText(path));
                    foreach (var item in cont) {
                        cds.Add(item);
                    }
                }

                IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
                locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
                _ = locator.FindReceiversAsync(CancellationToken.None);         // Fire the search process and wait for receiver found events in the handler. No await here!


                

            } catch (Exception ex) {
                // Log Error -> in GUI 
                radios.Add(new NamedUrl() { Name = "Init Error", ContentUrl = ex.Message });
            }
        }

        private void Receiver_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            foreach (var item in e.NewItems) {
                var rec = item as ChromecastReceiver;
                if (rec != null) {
                    // Autoselect -> TODO make pattern configurable
                    if (rec.Name.StartsWith("Bü")) {
                        SelectedReceiver = rec;
                    }
                }
            }
        }

        private void Locator_ChromecastReceivedFound(object sender, Sharpcaster.Models.ChromecastReceiver e) {
            Receiver.Add(e);
        }


        private async Task<QueueCaster.ChromecastClient> ConnectNewClient(ChromecastReceiver e) {
            QueueCaster.ChromecastClient ConnectedClient = QueueCaster.ChromecastClient.CreateQueueCasterClient(LoggerFactory);

            var st = await ConnectedClient.ConnectChromecast(e);
            Log.LogDebug("Connected available App[0]: {appid}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            var mediaChannel = ConnectedClient.GetChannel<QueueMediaChannel>();
            if (mediaChannel != null) {
                mediaChannel.QueueMediaStatusChanged += MediaChannel_QueueMediaStatusChanged;
                var rcChannel = ConnectedClient.GetChannel<StatusChannel<ReceiverStatusMessage, ChromecastStatus>>();
                if (rcChannel != null) {
                    rcChannel.StatusChanged += RcChannel_StatusChanged;
                }
    
                st = await ConnectedClient.LaunchApplicationAsync("9B5A75B4", true);    // TODO: APPID from config!

                ConnectedClient.Disconnected += ConnectedClient_Disconnected;
                Log.LogDebug("Launched/joined App[0]: {appId}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            }
            return ConnectedClient;
        }

        private void MediaChannel_QueueMediaStatusChanged(object sender, MediaStatusChangedEventArgs e) {
            QueueMediaChannel mc = sender as QueueMediaChannel;
            if (mc != null) {
                Log.LogTrace("MediaChanel Status changed: " + e.Status.FirstOrDefault()?.CurrentTime.ToString() ?? "<->");
            }
        }

        private void ConnectedClient_Disconnected(object sender, EventArgs e) {
            ChromecastClient cc = sender as ChromecastClient;
            if (cc != null) {
                Log.LogTrace("Disconnect received.");

                this.CastClients.Remove(SelectedReceiver.Name); //TODO: unsafe - kann ganz ein anderer sein !!!!!
                this.CastClients.Clear(); // Overkill, aber sicher ;-) - wenn man n cc connected hat muss man nach einem Disconnect alle neu connecten.....
            }
        }


        

        private void RcChannel_StatusChanged(object sender, EventArgs e) {
            StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = sender as StatusChannel<ReceiverStatusMessage, ChromecastStatus>;
            if (sc != null) {
                Log.LogTrace("Status changed: " + sc.Status.Volume.Level.ToString());

                dispatcherQueue.TryEnqueue(() => {
                    Volume = (int)(sc.Status.Volume.Level * 200);
                }); 
        
               
            }

        }

        public async Task CheckAndConnectChromecast() {
            if (SelectedReceiver != null) {
                ChromecastClient cc = null;
                if (!CastClients.ContainsKey(SelectedReceiver.Name)) {
                    cc = await ConnectNewClient(SelectedReceiver);
                    if (cc != null) {
                        CastClients.Add(SelectedReceiver.Name, cc);
                    }
                }
            }
        }



        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        
        public async void PlayRadio(NamedUrl radio) {
            await semaphoreSlim.WaitAsync();    // Only one Play at once is routet to LoadAsync!
            try {
                if (radio != null) {
                    if (SelectedReceiver != null) {
                        await CheckAndConnectChromecast();
                        if (CastClients.ContainsKey(SelectedReceiver.Name)) {
                            ChromecastClient cc = CastClients[SelectedReceiver.Name];
                            var mediaChannel = cc.GetChannel<QueueMediaChannel>();
                            if (mediaChannel != null) {
                                var media = new Media {
                                    ContentUrl = radio.ContentUrl,
                                    StreamType = StreamType.Live,
                                    ContentType = "audio/mp4",
                                    Metadata = new MediaMetadata() { Title = radio.Name ?? radio.ContentUrl }
                                };
                                Log.LogDebug("Load Media.");
                                await mediaChannel.LoadAsync(media);
                            }
                        }
                    }
                }
            } finally {
                semaphoreSlim?.Release();
            }
        }




        public async void PlayCd(Cd cd) {
            await semaphoreSlim.WaitAsync();    // Only one Play at once is routet to LoadAsync!
            try {
                if (cd != null) {
                    if (SelectedReceiver != null) {
                        await CheckAndConnectChromecast();
                        if (CastClients.ContainsKey(SelectedReceiver.Name)) {
                            ChromecastClient cc = CastClients[SelectedReceiver.Name];
                            var mediaChannel = cc.GetChannel<QueueMediaChannel>();
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
                                Log.LogDebug("Load Cd with " + media.Count + " tracks.");
                                await mediaChannel.QueueLoadAsync(media.ToArray());
                            }
                        }
                    }
                }
            } finally {
                semaphoreSlim?.Release();
            }
        }
    }
}
