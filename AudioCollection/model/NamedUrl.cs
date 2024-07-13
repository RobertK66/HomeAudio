using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi {
    public class NamedUrl {
        public String ContentUrl { get; set; }
        public String Name { get; set; }

        public NamedUrl(String name, String contentUrl) {
            ContentUrl = contentUrl;
            Name = name;
        }
    }
}
