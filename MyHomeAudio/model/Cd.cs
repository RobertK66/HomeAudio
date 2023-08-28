using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinGuiPackaged.model {
    public class Cd {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string CDID { get; set; }

        public List<NamedUrl> Tracks { get; set; } = new List<NamedUrl>();


    }
}
