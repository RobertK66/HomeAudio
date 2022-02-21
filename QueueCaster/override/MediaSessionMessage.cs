using Sharpcaster.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster {
    public class MediaSessionMessage : MessageWithId {
            /// <summary>
            /// Gets or sets the media session identifier
            /// </summary>
            [DataMember(Name = "mediaSessionId")]
            public long? MediaSessionId { get; set; }
        }
    }

