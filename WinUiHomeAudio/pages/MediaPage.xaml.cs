using ABI.System;
using AudioCollectionApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using WinUiHomeAudio.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using AudioCollectionApi.api;
using AudioCollectionApi.model;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio.pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaPage : VmPage {

        private ObservableCollection<IMedia>  _ListOfMedia = new ObservableCollection<IMedia>();
        public ObservableCollection<IMedia> ListOfMedia { get { return _ListOfMedia; } set { if (_ListOfMedia != value) { _ListOfMedia = value; RaisePropertyChanged(); } } }
        
        public MediaPage() {
            this.InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            //var p = e.Parameter;
            ListOfMedia = App.Host.Services.GetRequiredService<IMediaRepository>().GetMediaRepository(e?.Parameter?.ToString() ?? "X");
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            IMedia? item = (args.InvokedItem as IMedia);
            if (item != null) {
                Debug.WriteLine(item.Name);
                if ((item.IsCollection) && (item is Cd cd)) {
                    App.Host.Services.GetRequiredService<ChromeCastRepository>().PlayCed(cd);
                } else if (item is NamedUrl radio){
                    App.Host.Services.GetRequiredService<ChromeCastRepository>().PlayRadio(radio);
                }
            }
        }
    }
}
