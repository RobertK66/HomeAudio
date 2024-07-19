using AudioCollectionApi.api;
using AudioCollectionApi.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi {
    public class NamedUrl :BaseMedia, IMedia{
        public required String ContentUrl { get; set; }
        //public String Name { get; set; }

        public bool IsCollection => false;

        public IList<IMedia> Content => (new List<IMedia>() { this });

        public NamedUrl(String name, String contentUrl) {
            ContentUrl = contentUrl;
            Name = name;
        }
    }
}
