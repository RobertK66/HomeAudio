﻿using Sharpcaster.Messages;
using System.Runtime.Serialization;

namespace QueueCaster {
    /// <summary>
    /// Load message
    /// </summary>
    [DataContract]
    class LoadMessage : MessageWithSession {
        /// <summary>
        /// Gets or sets the media
        /// </summary>
        [DataMember(Name = "media")]
        public Sharpcaster.Models.Media.Media? Media { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the media must be played directly or not
        /// </summary>
        [DataMember(Name = "autoplay")]
        public bool AutoPlay { get; set; }
    }
}
