using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUiHomeAudio.pages
{
    public class NavContext
    {
        public required String category { get; set; }
        public IPlayerProxy? player {  get; set; }


    }
}
