using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCollectionApi.model;

namespace WinUiHomeAudio.pages {

    public class MyItemTemplateSelector : DataTemplateSelector {
        public DataTemplate CdTemplate { get; set; }
        public DataTemplate RadioTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item) {
            if (item is Cd) {
                return CdTemplate;
            } else {
                return RadioTemplate;
            }
        }
    }
}