using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Markup;

namespace MyHomeAudio.nav
{
    public class CategoryBase { }

    public class Category : CategoryBase {
        public string Name { get; set; }
        public string Tag { get; set; }
        public Symbol Glyph { get; set; }
    }

    //public class Separator : CategoryBase { }

    //public class Header : CategoryBase {
    //    public string Name { get; set; }
    //}

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

