using ABI.System;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Sharpcaster.Models;
using Sharpcaster.Models.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeAudio.model
{
    public class ChromeCastRepository {

        private ChromeCastClientWrapper? _activeClient = null;
        private ObservableCollection<ChromeCastClientWrapper> KnownChromecasts = new ObservableCollection<ChromeCastClientWrapper>();
        private string? _autoConnectName;
        private string _appId;
        private ILoggerFactory _loggerFactory;
        private ILogger<ChromeCastRepository> Log;

        public ChromeCastRepository(AppSettings appSettings, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) {
            _autoConnectName = appSettings.AutoConnectName;
            _appId = appSettings.AppId;
            _loggerFactory = loggerFactory;
            Log = loggerFactory.CreateLogger<ChromeCastRepository>();
        }

        public void Add(ChromecastReceiver e) {
            Log.LogInformation("Receiver '{CcrName}' found at {CcrUri}", e.Name, e.DeviceUri);
            var dq = DispatcherQueue.GetForCurrentThread();
            var ccc = new ChromeCastClientWrapper(e, dq, _loggerFactory);
            KnownChromecasts.Add(ccc);
            

            if (!String.IsNullOrEmpty(_autoConnectName) && e.Name.StartsWith(_autoConnectName)) {
                Log.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", e.Name);
                _ = TryConnectAsync(ccc);
            }
        }

        public async Task TryConnectAsync(ChromeCastClientWrapper ccc) {
            var status = await ccc.TryConnectAsync(_appId);
            if (status) {
                SetActiveClient(ccc);
            }
        }

        public ObservableCollection<ChromeCastClientWrapper> GetClients() {
            return KnownChromecasts;
        }

        internal void PlayCed(Cd cd) {
            _activeClient?.PlayCdAsync(cd);
        }

        internal void PlayRadio(NamedUrl url) {
            _activeClient?.PlayRadioAsync(url);
        }

        internal void SetActiveClient(ChromeCastClientWrapper selectedCcc) {
            _activeClient = selectedCcc;
            App.Current.m_window.ActiveCcc = selectedCcc;
        }

        internal void VolumeUp() {
            _activeClient?.VolumeUp();
        }
    }
}
