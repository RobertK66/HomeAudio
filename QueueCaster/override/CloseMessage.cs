using Sharpcaster.Interfaces;
using Sharpcaster.Messages;
using System.Runtime.Serialization;

namespace QueueCaster {
    
    /// <summary>
    /// Close message
    /// </summary>
    [DataContract]
    [ReceptionMessage]
    class CloseMessage : Message {
    }
    
}