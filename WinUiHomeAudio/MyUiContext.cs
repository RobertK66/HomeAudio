using AudioCollectionApi.api;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUiHomeAudio.model;

namespace WinUiHomeAudio {
    public class MyUiContext  : IObservableContext<IPlayerProxy> {
        public DispatcherQueue? dq {  get; set; }

        public void InvokePropChanged(PropertyChangedEventHandler propertyChanged, IPlayerProxy player, EventArgs propertyChangedEventArgs) {
            _ = dq?.TryEnqueue(() => propertyChanged(this, propertyChangedEventArgs as PropertyChangedEventArgs ?? new PropertyChangedEventArgs("?")));
        }
    }
}
