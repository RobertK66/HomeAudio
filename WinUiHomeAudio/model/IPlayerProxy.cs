using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUiHomeAudio.model {
    public interface IPlayerProxy {
        String Name { get; set; }

        Task<bool> TryConnectAsync(string appId);
    }
}
