using AudioCollectionApi.model;
using Sharpcaster.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUiHomeAudio.model {
    public interface IPlayerRepository {

        public event EventHandler<IPlayerProxy> PlayerFound;

        ObservableCollection<ChromeCastClientWrapper> KnownPlayer { get; }
        

        void PlayCd(Cd cd);
        void PlayRadio(NamedUrl radio);
        void SetActiveClient(IPlayerProxy? value);
        Task LoadAllAsync();
        Task TryConnectAsync(IPlayerProxy ccw);
    }
}
