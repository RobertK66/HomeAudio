using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi {
    public class MediaCategory {
        public string Id { get; private set; }
        public string? Name { get; set; }

        public MediaCategory(string id) {
            Id = id;
        }
    }
}
