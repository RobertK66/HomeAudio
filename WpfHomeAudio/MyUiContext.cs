using AudioCollectionApi.api;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace WpfHomeAudio {


    // IN WPF we can use BindingOperations.EnableCollectionSynchronization !
    [Obsolete]
    public class MyUiContext  : IObservableContext {
        private Dispatcher dispatcher;

        public MyUiContext(Dispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }
        public void InvokePropChanged<T>(PropertyChangedEventHandler propertyChanged, T player, PropertyChangedEventArgs propertyChangedEventArgs) {
            dispatcher.Invoke(() => propertyChanged(player, propertyChangedEventArgs));
            //propertyChanged(player, pcea);

        }
    }
}
