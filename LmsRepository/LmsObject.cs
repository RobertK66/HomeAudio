using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsRepositiory {
    public class LmsObject :IMedia {
        public String Id { get; set; }
        public String Name { get; set; }

        public bool IsCollection => false;

        public IList<IMedia> Content => new List<IMedia>();
            
        public string? ContentUrl => Id;

        public LmsObject(String id, String name) {
            Id = id;
            Name = name;
        }
    }
}
