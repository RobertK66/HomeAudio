
using QueueCaster.queue.models;
using Sharpcaster.Messages;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QueueCaster.queue.messages {

    [DataContract]
    [ReceptionMessage]
    public class QueueItemsMessage : MediaSessionMessage {

        [DataMember(Name = "items")]
        public QueueItem[]? Items { get; set; }
    }
}
