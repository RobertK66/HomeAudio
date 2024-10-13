using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Sharpcaster;
using Sharpcaster.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WinUiHomeAudio.model {
    public class ChromeCastRepository : IPlayerRepository, IDisposable {

        // Used from UIBinding
        public ChromeCastClientWrapper? _activeClient = null;
      
        private string? _autoConnectName;
        public string _appId;
        private ILoggerFactory _loggerFactory;
        private ILogger<ChromeCastRepository> Log;

        // Used from UIBinding
        private ObservableCollection<IPlayerProxy> _knownPlayer = new();

        public event EventHandler<IPlayerProxy>? PlayerFound;

        public ObservableCollection<IPlayerProxy> KnownPlayer { get => _knownPlayer; }

        public ChromeCastRepository(AppSettings appSettings, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) {
            _autoConnectName = appSettings.AutoConnectName;
            _appId = appSettings.AppId;
            _loggerFactory = loggerFactory;
            Log = loggerFactory.CreateLogger<ChromeCastRepository>();
            Log.LogTrace("ChromecastRepository Constructor finished.");
        }

        public ChromeCastClientWrapper Add(ChromecastReceiver e) {
            Log.LogInformation("Receiver '{CcrName}' found at {CcrUri} {tostr}", e.Name, e.DeviceUri, e.Port);
            var dq = DispatcherQueue.GetForCurrentThread();
            var ccc = new ChromeCastClientWrapper(e, dq, _loggerFactory);
            KnownPlayer.Add(ccc);
            return ccc;

            //if (!String.IsNullOrEmpty(_autoConnectName) && e.Name.StartsWith(_autoConnectName)) {
            //    Log.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", e.Name);
            //    _ = TryConnectAsync(ccc);
            //}
        }

        public async Task TryConnectAsync(IPlayerProxy pp) {
            
            var status = await pp.TryConnectAsync(_appId);
            if (status) {
                SetActiveClient(pp);
            }
        }

        //public void PlayCd(IMedia cd) {
        //    _activeClient?.PlayCdAsync(cd as Cd);
        //}

        //public void PlayRadio(IMedia url) {
        //    _activeClient?.PlayRadioAsync(url as NamedUrl);
        //}

        public void SetActiveClient(IPlayerProxy? selectedCcc) {
            _activeClient = selectedCcc as ChromeCastClientWrapper;
        }

        public void VolumeUp() {
            _activeClient?.VolumeUp();
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) { // release other disposable objects
                Log.LogInformation("Dispose called");
                // Lets do some time consuming stuff here...
                //var t = Task.Delay(2000);
                //t.Wait();
                Log.LogInformation("Dispose finished");
            }
        }

        public ObservableCollection<IPlayerProxy> GetKnownPlayer() {
            return KnownPlayer;
        }

        public async Task LoadAllAsync() {

            // start the search for CC Receivers in local network
            MdnsChromecastLocator locator = new();
            locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
            // Fire the search process and allow 10 seconds for receiver found events to call the handler.
            CancellationTokenSource tokenSource = new CancellationTokenSource(10000);
            await locator.FindReceiversAsync(tokenSource.Token);          

        }

        private void Locator_ChromecastReceivedFound(object? sender, ChromecastReceiver e) {
            var wrapper = Add(e);
            PlayerFound?.Invoke(sender, wrapper);
        }

        
        public void VolumeDown() {
            _activeClient?.VolumeDown();
        }
    }
}
