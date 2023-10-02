using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeAudio.model {
    public class Cd {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string CDID { get; set; }

        private string _pp;
        public string Picpath {
            
            get {
                return _pp;
            }
            set {
                if (value != null) {
                    _pp = value;
                    Image = new BitmapImage(new Uri(value));
                }
            }
        }


        public BitmapImage Image { get; set; }

        public List<NamedUrl> Tracks { get; set; } = new List<NamedUrl>();


    }
}
