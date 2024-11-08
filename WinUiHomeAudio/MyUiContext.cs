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
    public class MyUiContext  : IObservableContext {
        public DispatcherQueue? dq {  get; set; }

        public void InvokePropChanged<T>(PropertyChangedEventHandler propertyChanged, T player, PropertyChangedEventArgs eventArgs) {
            //PropertyChangedEventArgs? eventArgs = propertyChangedEventArgs as PropertyChangedEventArgs;
            //if (eventArgs != null) {
                _ = dq?.TryEnqueue(() => propertyChanged(this, eventArgs));
            //}
        }

    }
}
