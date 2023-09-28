using ABI.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyHomeAudio.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Chat;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CdPage : VmPage {

     
        private ObservableCollection<Cd>  _ListOfCDs;
        public ObservableCollection<Cd> ListOfCDs { get { return _ListOfCDs; } set { if (_ListOfCDs != value) { _ListOfCDs = value; RaisePropertyChanged(); } } }
        
        public CdPage() {
            this.InitializeComponent();
        }

     
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var p = e.Parameter;
            ListOfCDs = App.Current.MyHost.Services.GetRequiredService<MediaRepository>().GetCdRepository(e.Parameter.ToString());
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            Cd cd = (args.InvokedItem as Cd);
            if (cd != null) {
                Debug.WriteLine(cd.Name);
                App.Current.ChromeCastRepos.PlayCed(cd);
            } 

        }
    }
}
