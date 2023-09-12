using ABI.System;
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


        private ChromeCastClient _activeClient = null;
        private ObservableCollection<ChromeCastClient> KnownChromecasts = new ObservableCollection<ChromeCastClient>();

        public void Add(ChromecastReceiver e) {
            var dq = DispatcherQueue.GetForCurrentThread();
            var ccc = new ChromeCastClient(e, dq);
            KnownChromecasts.Add(ccc);
            //}
            if (e.Name.StartsWith("Bü")) {
                _ = ccc.TryConnectAsync();
            }
        }

        public ObservableCollection<ChromeCastClient> GetClients() {
            return KnownChromecasts;
        }

        internal void PlayCed(Cd cd) {
            _activeClient?.PlayCdAsync(cd);
        }

        internal void PlayRadio(NamedUrl url) {
            _activeClient?.PlayRadioAsync(url);
        }

        internal void SetActiveClient(ChromeCastClient selectedCcc) {
            _activeClient = selectedCcc;
        }

        internal void VolumeUp() {
            _activeClient?.VolumeUp();
        }
    }
}
