using Sharpcaster.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster
{
    [DataContract]
    [ReceptionMessage]
    internal class LoadCancelledMessage : MessageWithId {
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) {
//            throw new Exception("Load cancelled");
        }
    }

}
