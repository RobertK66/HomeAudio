using Sharpcaster.Messages;
using Sharpcaster.Messages.Chromecast;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QueueCaster {
    /// <summary>
    /// Media status message
    /// </summary>
    [DataContract]
    [ReceptionMessage]
    public class MediaStatusMessage : StatusMessage<IEnumerable<MediaStatus>> {
    }
}
