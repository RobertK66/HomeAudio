using Sharpcaster.Messages;
using Sharpcaster.Messages.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster
{
    // We need this media channel message to get Media - Stop transmitted for a media session (its private in Sharpcaster library)
    // Attention there is a Publich Sharpcaster StopMessage() this is used on receiver Channel for global session stop !!!!?

    [DataContract]
    class StopMessage : MediaSessionMessage {
    }
}
