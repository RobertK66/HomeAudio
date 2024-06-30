using Sharpcaster.Messages.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster
{
  /// <summary>
    /// Pause message
    /// </summary>
    [DataContract]
    class PauseMessage : MediaSessionMessage {
    }

}
