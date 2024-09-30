using System;
using System.Collections.Generic;
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
        String? AppId { get; set; }
        bool IsConnected { get; set; }
        bool IsOn { get; set; }


        Task<bool> TryConnectAsync(string appId);
    }
}
