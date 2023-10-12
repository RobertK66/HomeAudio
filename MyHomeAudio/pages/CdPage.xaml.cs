//using ABI.System;
using AudioCollectionApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using MyHomeAudio.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

namespace MyHomeAudio.pages
{
    public sealed class StringToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, string culture) {
            try {
                if (value != null) {
                    return new BitmapImage(new Uri((string)value));
                }
                return new BitmapImage();
            } catch {
                return new BitmapImage();
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CdPage : VmPage {

     
        private ObservableCollection<Cd>  _ListOfCDs = new();
        public ObservableCollection<Cd> ListOfCDs { get { return _ListOfCDs; } set { if (_ListOfCDs != value) { _ListOfCDs = value; RaisePropertyChanged(); } } }
        
        public CdPage() {
            this.InitializeComponent();
        }

     
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (e.Parameter is string p) {
                ListOfCDs = App.Services.GetRequiredService<IMediaRepository>().GetCdRepository(p);
            }
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            Cd? cd = (args.InvokedItem as Cd);
            if (cd != null) {
                Debug.WriteLine(cd.Name);
                App.Services.GetRequiredService<ChromeCastRepository>().PlayCed(cd);
            } 

        }
    }
}
