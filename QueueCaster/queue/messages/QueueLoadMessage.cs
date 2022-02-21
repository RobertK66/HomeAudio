using QueueCaster.queue.models;
using Sharpcaster.Messages;
using System.Runtime.Serialization;

namespace QueueCaster.queue.messages {

    [DataContract]
    public class QueueLoadMessage : MessageWithSession
    {
        [DataMember(Name = "items")]
        public QueueItem[]? Items { get; set; }
    }
  
}
