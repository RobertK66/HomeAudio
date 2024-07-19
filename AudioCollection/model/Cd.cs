using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi {

  
    public class Cd :IMedia {
        private static int UnknownIdCont = 0;

        public string? Name { get; set; }
        public string? Artist { get; set; }
        public string CDID { get; set; }

        public string? Picpath { get; set; }

        public string? ContentUrl => null;

        public List<NamedUrl> Tracks { get; set; } = new List<NamedUrl>();

        public bool IsCollection => true;

        public IList<IMedia> Content => (IList<IMedia>)Tracks;

        //public IEnumerator<IMedia> GetContent() => Tracks.GetEnumerator();

        public Cd(string Id) {
            CDID = Id;
        }

        public Cd() {
            CDID = $"<unknown_"+(UnknownIdCont++)+">";
        }
    }
}
