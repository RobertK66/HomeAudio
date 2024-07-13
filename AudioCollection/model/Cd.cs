using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi {

  
    public class Cd {
        private static int UnknownIdCont = 0;

        public string? Name { get; set; }
        public string? Artist { get; set; }
        public string CDID { get; set; }

        public string? Picpath { get; set; }

        public List<NamedUrl> Tracks { get; set; } = new List<NamedUrl>();

        public Cd(string Id) {
            CDID = Id;
        }

        public Cd() {
            CDID = $"<unknown_"+(UnknownIdCont++)+">";
        }
    }
}
