using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCollectionApi {
 
    public class NamedUrl :BaseMedia, IMedia{
        public required String ContentUrl { get; set; }
        //public String Name { get; set; }

        public bool IsCollection => false;

        public IList<IMedia> Content => (new List<IMedia>() { this });

        [SetsRequiredMembers]
        public NamedUrl(String name, String contentUrl) {
            ContentUrl = contentUrl;
            Name = name;
        }
    }
}
