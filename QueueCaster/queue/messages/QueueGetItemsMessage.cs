using System.Runtime.Serialization;

namespace QueueCaster.queue.messages {

    [DataContract]
    public class QueueGetItemsMessage : MediaSessionMessage {
        [DataMember(Name = "itemIds")]
        public int[]? Ids { get; set; }
    }


}