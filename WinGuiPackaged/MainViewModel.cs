using Microsoft.Extensions.Logging;
using QueueCaster;
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
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Web.UI;
using WinGuiPackaged.logger;
using WinGuiPackaged.model;

namespace WinGuiPackaged {

    public class MainViewModel : IRadioViewModel, ICdViewModel {


        public ILoggerFactory LoggerFactory { get; set; }

        private logger.LoggerVm _loggerVM;
        public logger.LoggerVm LogWindowViewModel {
            get { return _loggerVM; }
            set {
                _loggerVM = value;
        } }

        

        private ILogger Log;


        private ObservableCollection<Cd> cds = new();
        public ObservableCollection<Cd> Cds { get { return cds; } }
        public Cd SelectedCd { get; set; } = null;


        private ObservableCollection<NamedUrl> radios = new();
        public ObservableCollection<NamedUrl> WebRadios { get { return radios; } }
        public NamedUrl SelectedRadio { get; set; } = null;


        private ObservableCollection<ChromecastReceiver> receiver = new();
        public ObservableCollection<ChromecastReceiver> Receiver { get { return receiver; } }
        public ChromecastReceiver SelectedReceiver { get; set; } = null;


        private Dictionary<String, ChromecastClient> CastClients = new Dictionary<String, ChromecastClient>();

        public MainViewModel(ILoggerFactory loggerFactory, logger.LoggerVm logVm) {
            try {
                LoggerFactory = loggerFactory;
                Log = LoggerFactory.CreateLogger<MainViewModel>();
                LogWindowViewModel = logVm;
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
                _ = locator.FindReceiversAsync(CancellationToken.None);         // Fire the search process and wait for receiver found events!


            } catch (Exception ex) {
                // Log Error -> in GUI 
                radios.Add(new NamedUrl() { Name = "Init Error", ContentUrl = ex.Message });
            }
        }

        private void Receiver_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            foreach (var item in e.NewItems) {
                var rec = item as ChromecastReceiver;
                if (rec != null) {
                    if (rec.Name.StartsWith("Bü")) {
                        SelectedReceiver = rec;
                    }
                }
            }
        }

        private void Locator_ChromecastReceivedFound(object sender, Sharpcaster.Models.ChromecastReceiver e) {
            Receiver.Add(e);
        }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async void PlayRadio(NamedUrl radio) {
            await semaphoreSlim.WaitAsync();
            try {
                if (radio != null) {
                    if (SelectedReceiver != null) {
                        ChromecastClient cc = null;
                        if (!CastClients.ContainsKey(SelectedReceiver.Name)) {
                            cc = await ConnectNewClient(SelectedReceiver);
                            if (cc != null) {
                                CastClients.Add(SelectedReceiver.Name, cc);
                            }
                        }
                        if (CastClients.ContainsKey(SelectedReceiver.Name)) {
                            cc = CastClients[SelectedReceiver.Name];
                            var mediaChannel = cc.GetChannel<QueueMediaChannel>();
                            if (mediaChannel != null) {
                                var media = new Media {
                                    ContentUrl = radio.ContentUrl,
                                    StreamType = StreamType.Live,
                                    ContentType = "audio/mp4",
                                    Metadata = new MediaMetadata() { Title = radio.Name ?? radio.ContentUrl }
                                };
                                Log.LogDebug("Load Media.");
                                _ = await mediaChannel.LoadAsync(media);
                            }
                        }
                    }
                }
            } finally {
                semaphoreSlim?.Release();
            }
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
                    //StatusChannel<ReceiverStatusMessage, ChromecastStatus> sc = (StatusChannel<ReceiverStatusMessage, ChromecastStatus>)rcChannel;
                    rcChannel.StatusChanged += RcChannel_StatusChanged;
                }

                st = await ConnectedClient.LaunchApplicationAsync("9B5A75B4", true);

                ConnectedClient.Disconnected += ConnectedClient_Disconnected;
                Log.LogDebug("Launched/joined App[0]: {appId}", (((st?.Applications?.Count ?? 0) > 0) ? st?.Applications[0].AppId : "<null>"));
            }
            return ConnectedClient;
        }

        private void MediaChannel_QueueMediaStatusChanged(object sender, EventArgs e) {

        }

        private void ConnectedClient_Disconnected(object sender, EventArgs e) {

        }

        private void RcChannel_StatusChanged(object sender, EventArgs e) {

        }
    }
}
