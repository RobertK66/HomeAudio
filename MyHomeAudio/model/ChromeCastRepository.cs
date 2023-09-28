using ABI.System;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Sharpcaster.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeAudio.model
{
    public class ChromeCastRepository {

        private ChromeCastClientWrapper _activeClient = null;
        private ObservableCollection<ChromeCastClientWrapper> KnownChromecasts = new ObservableCollection<ChromeCastClientWrapper>();
        private string _autoConnectName;
        private string _appId;
        private ILoggerFactory _loggerFactory;  

        public ChromeCastRepository(string autoConnectName, string appId, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) {
            _autoConnectName = autoConnectName;
            _appId = appId;
            _loggerFactory = loggerFactory;
        }

        public void Add(ChromecastReceiver e) {
            var dq = DispatcherQueue.GetForCurrentThread();
            var ccc = new ChromeCastClientWrapper(e, dq, _loggerFactory);
            KnownChromecasts.Add(ccc);
            
            if (e.Name.StartsWith(_autoConnectName)) {
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
