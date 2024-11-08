using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AudioCollectionApi.api {
    public interface IPlayerProxy {
        String Name { get; }
        String Id { get; }

        String Status { get; set; }
        int Volume { get; set; }
        String? MediaStatus { get; set; }
        //String? AppId { get; set; }
        bool IsConnected { get; set; }          
        bool IsOn { get; set; }


        void Disconnect();
        Task<bool> TryConnectAsync(string appId);
        void VolumeDown();
        void VolumeUp();
        void PlayCd(IMedia cd);
        void PlayRadio(IMedia radio);
        void Stop();
        void Play();

        
        void SetContext(IObservableContext myContext);
    }
}
