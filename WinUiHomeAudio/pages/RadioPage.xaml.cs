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
    public sealed partial class RadioPage : VmPage {

        private ObservableCollection<IMedia>  _ListOfRadios = new ObservableCollection<IMedia>();
        public ObservableCollection<IMedia> ListOfRadios { get { return _ListOfRadios; } set { if (_ListOfRadios != value) { _ListOfRadios = value; RaisePropertyChanged(); } } }
        
        public RadioPage() {
            this.InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var p = e.Parameter;

            ListOfRadios = App.Host.Services.GetRequiredService<IMediaRepository2>().GetMediaRepository(e?.Parameter?.ToString() ?? "X");
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            IMedia? radio = (args.InvokedItem as IMedia);
            if (radio != null) {
                Debug.WriteLine(radio.Name);
                if (radio.IsCollection) {
                    App.Host.Services.GetRequiredService<ChromeCastRepository>().PlayCed(radio as Cd);
                } else {
                    App.Host.Services.GetRequiredService<ChromeCastRepository>().PlayRadio(radio as NamedUrl);
                }
            }
        }
    }
}
