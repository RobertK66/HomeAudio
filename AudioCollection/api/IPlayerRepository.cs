using AudioCollectionApi.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi.api {
    public interface IPlayerRepository {

        public event EventHandler<IPlayerProxy> PlayerFound;

        ObservableCollection<IPlayerProxy> KnownPlayer { get; }
        

        //void PlayCd(IMedia cd);
        //void PlayRadio(IMedia radio);
        //void SetActiveClient(IPlayerProxy? value);
        Task LoadAllAsync();
        Task TryConnectAsync(IPlayerProxy ccw);
        //void VolumeUp();
        //void VolumeDown();
    }
}
