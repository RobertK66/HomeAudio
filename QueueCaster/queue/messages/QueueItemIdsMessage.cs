﻿using Sharpcaster.Messages;
using Sharpcaster.Messages.Media;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QueueCaster.queue.messages {
    [DataContract]
    [ReceptionMessage]
    public class QueueItemIdsMessage : MediaSessionMessage {
        
        [DataMember(Name = "itemIds")]
        public int[]? Ids { get; set; }
    }
}
