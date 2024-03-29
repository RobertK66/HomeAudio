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
using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT.Interop;          // Needed for XAML/HWND interop.
using Windows.UI;
using System.Net.NetworkInformation;
using MyHomeAudio.pages;
using Windows.Storage;
using System.Diagnostics;
using Microsoft.UI.Xaml.Shapes;

//using AppUIBasics.Common;
//using AppUIBasics.Data;
//using AppUIBasics.Helper;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Windows.ApplicationModel;
//using Windows.ApplicationModel.Activation;
//using Windows.ApplicationModel.Core;
//using Windows.Foundation.Metadata;
//using Windows.System.Profile;
//using WinUIGallery.DesktopWap.DataModel;
//using WASDK = Microsoft.WindowsAppSDK;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>


    public sealed partial class MainWindow : Window {

        public NavigationView MainNavPane { get => this.MainNavView; }

        public MainWindow() {
            this.InitializeComponent();
            AppWindow.Title = "My Audio - Cast Application";
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            AppWindow.TitleBar.BackgroundColor = Colors.Bisque;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Bisque;

            var isLeft = ApplicationData.Current.LocalSettings.Values[AppSettingKeys.IsLeftMode];
            if (isLeft == null || ((bool)isLeft == true)) {
                MainNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            } else {
                MainNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            }

            String theme = ApplicationData.Current.LocalSettings.Values[AppSettingKeys.UiTheme]?.ToString();
            if (theme != null) {
                var t = App.GetEnum<ElementTheme>(theme);

                if (this.Content is FrameworkElement rootElement) {
                    rootElement.RequestedTheme = t;
                }
            }

            var repPath = ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath]?.ToString();
            if (repPath == null) {
                repPath = ApplicationData.Current.LocalFolder.Path;
            }
            BuildMenue(repPath);


        }

        public void BuildMenue(string repPath) {
            MainNavView.MenuItems.Clear();
            int i = 0;
            foreach(var f in Directory.GetFiles(repPath, "*.json")) {
                if (File.ReadAllText(f).Contains("\"Tracks\"")) {
                    MainNavView.MenuItems.Add(new NavigationViewItem() { Icon = new FontIcon() { Glyph = "\uEA3F" }, Content= System.IO.Path.GetFileName(f), Tag = "CD-" + i++ });
                } else {
                    MainNavView.MenuItems.Add(new NavigationViewItem() { Icon = new FontIcon() { Glyph = "\uE704" }, Content = System.IO.Path.GetFileName(f), Tag = "Radio-" + i++ });
                }
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                sender.AlwaysShowHeader = false;
                ContentFrame.Navigate(typeof(SettingsPage));
                //ContentFrame.Content = new SettingsPage();
            } else {
                sender.AlwaysShowHeader = true;
                string selectedItem = (String)args.InvokedItem;
                sender.Header = "Sample Page " + selectedItem;
                ContentFrame.Navigate(typeof(ContentPage), selectedItem);
                //ContentFrame.Content = new ContentPage();
            }
        }

    }
}
