using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi {
    public class MediaCategory  {
        public string Id { get; private set; }
        public string? Name { get; set; }
        //public bool IsCollection => true;
        //public List<IMedia> Entries { get; set; } = new List<IMedia>();

        //public IList<IMedia> Content => Entries;

        public MediaCategory(string id) {
            Id = id;
        }
    }
}
