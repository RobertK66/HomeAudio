using AudioCollectionApi.api;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WinUiHomeAudio {
    public class MyUiContext  : IObservableContext {
        public DispatcherQueue? dq {  get; set; }

        public void Execute(Action value) {
            _ = dq?.TryEnqueue(() => value());
        }

        public void InvokePropChanged<T>(PropertyChangedEventHandler propertyChanged, T player, PropertyChangedEventArgs eventArgs) {
           _ = dq?.TryEnqueue(() => propertyChanged(player, eventArgs));
        }

    }
}
