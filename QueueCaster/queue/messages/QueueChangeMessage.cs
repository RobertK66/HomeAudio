
using Sharpcaster.Messages;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QueueCaster.queue.messages {

    [DataContract]
    [ReceptionMessage]
    public class QueueChangeMessage : MessageWithSession
    {
        [DataMember(Name = "changeType")]
        public string? ChangeType { get; set; }

        [DataMember(Name = "itemIds")]
        public int[]? ChangedIds { get; set; }
    }


}
