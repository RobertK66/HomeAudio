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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DataTemplate ItemTemplate { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                              //    public DataTemplate SeperatorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item) {
            //return item is Separator ? SeperatorTemplate : item is Header ? ItemTemplate : ItemTemplate;
            return ItemTemplate;
        }
    }
}
