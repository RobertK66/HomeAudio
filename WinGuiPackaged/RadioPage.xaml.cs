using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinGuiPackaged {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RadioPage : Page {

        private IRadioViewModel AnyViewModel; // = new MainViewModel();
        private ILogger<RadioPage> Logger;

        public RadioPage() {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            AnyViewModel = e.Parameter as IRadioViewModel;
            Logger = AnyViewModel.LoggerFactory.CreateLogger<RadioPage>();
            base.OnNavigatedTo(e);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //myButton.Content = "....";
            Logger.LogInformation("Selection changed");

        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
            //myButton.Content = AnyViewModel.SelectedRadio?.Name;
            Logger.LogInformation("Play " + AnyViewModel.SelectedRadio?.Name);

        }

        private void ListView_AccessKeyDisplayDismissed(UIElement sender, AccessKeyDisplayDismissedEventArgs args) {

        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            Logger.LogInformation("Double tapped");
        }
    }
}
