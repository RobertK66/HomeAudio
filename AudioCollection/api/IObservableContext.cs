using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi.api {
    public interface IObservableContext {
        void InvokePropChanged<T>(PropertyChangedEventHandler propertyChanged, T record, PropertyChangedEventArgs changedEventArgs);
    }
}
