using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Markup;

namespace WinUiHomeAudio {

    [ContentProperty(Name = "ItemTemplate")]
    class MenuItemTemplateSelector : DataTemplateSelector {
        public DataTemplate ItemTemplate { get; set; }
        //    public DataTemplate SeperatorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item) {
            //return item is Separator ? SeperatorTemplate : item is Header ? ItemTemplate : ItemTemplate;
            return ItemTemplate;
        }
    }
}
