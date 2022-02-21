using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace QueueCaster.queue.models {
    [DataContract]
    public class QueueItem {
        [DataMember(Name = "itemId", EmitDefaultValue = false)]
        public int? ItemId { get; set; }

        [DataMember(Name = "media")]
        public Media? Media { get; set; }

        //[DataMember(Name = "autoPlay")]
        //public bool IsAutoPlay { get; set; }

        [DataMember(Name = "orderId")]
        public long OrderId { get; set; }

        [DataMember(Name = "startTime")]
        public long StartTime { get; set; }

        //[DataMember(Name = "preloadTime")]
    }
}
