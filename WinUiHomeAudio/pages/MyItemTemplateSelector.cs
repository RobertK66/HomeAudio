using AudioCollectionApi.model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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