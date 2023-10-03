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
using MyHomeAudio.nav;
using System.Collections.ObjectModel;
using System.Collections;
using MyHomeAudio.model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyHomeAudio.logger;
using Microsoft.Extensions.DependencyInjection;
using AudioCollectionApi;

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

namespace MyHomeAudio
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>


    public sealed partial class MainWindow : Window, INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public NavigationView MainNavPane { get => this.MainNavView; }

        public LoggerVm LoggerVm { get; set; }

        public ObservableCollection<CategoryBase> Categories = new ObservableCollection<CategoryBase>();

        public ObservableCollection<CategoryBase> FooterCategories = new ObservableCollection<CategoryBase>();


        private ChromeCastClientWrapper? _selectedCCC;
        public ChromeCastClientWrapper? ActiveCcc { get { return _selectedCCC; } set { if (_selectedCCC != value) { _selectedCCC = value; RaisePropertyChanged(); } } }

        private string currentConfigPath = "";



        public MainWindow() {
            this.InitializeComponent();

            AppSettings appSettings = App.Services.GetRequiredService<AppSettings>();

            AppWindow.Title = "My Audio - Cast Application";
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            AppWindow.TitleBar.BackgroundColor = Colors.Bisque;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Bisque;

            LoggerVm = App.Services.GetRequiredService<LoggerVm>();

            if (appSettings.IsLeftMode) {
                MainNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            } else {
                MainNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            }

            var t = App.GetEnum<ElementTheme>(appSettings.UiTheme);
            if (this.Content is FrameworkElement rootElement) {
                rootElement.RequestedTheme = t;
            }

            FooterCategories.Add(new Category() { Glyph = Symbol.AlignLeft, Name = "ChromeCast", Tag = "CC" });
            BuildMenue(appSettings.ReposPath);
        }

        public void BuildMenue(string repPath) {
            if (!repPath.Equals(currentConfigPath)) {
                currentConfigPath = repPath;
                Categories.Clear();
                IMediaRepository mr = App.Services.GetRequiredService<IMediaRepository>();
                mr.LoadAll(repPath);
                
                foreach(var c in mr.GetRadioCategories()) {
                    Categories.Add(new Category() { Glyph = Symbol.Target, Name = c.Name, Tag = c.Id });
                }

                foreach (var c in mr.GetCdCategories()) {
                    Categories.Add(new Category() { Glyph = Symbol.Target, Name = c.Name, Tag = c.Id });
                }
           
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                //this.ccPlayer.Visibility = Visibility.Collapsed;
                ContentFrame.Navigate(typeof(SettingsPage));
            } else {
                //this.ccPlayer.Visibility = Visibility.Visible;
                string selectedItem = (String)args.InvokedItem;
                if (selectedItem.Equals("ChromeCast")) {
                    ContentFrame.Navigate(typeof(ChromecastPage));
                } else if (selectedItem.Contains("Cd")) {
                    ContentFrame.Navigate(typeof(CdPage), args.InvokedItemContainer.Tag.ToString());
                } else {
                    ContentFrame.Navigate(typeof(RadioPage), args.InvokedItemContainer.Tag.ToString());
                }
            }
        }

        private void ccPlayer_VolumeUp(object sender, EventArgs e) {
            ActiveCcc?.VolumeUp();
        }

        private void ccPlayer_VolumeDown(object sender, EventArgs e) {
            ActiveCcc?.VolumeDown();
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args) {
            if (ContentFrame?.Content == null) {
                ContentFrame?.Navigate(typeof(RadioPage), ((Category?)Categories.Where(c => ((Category)c).Tag.StartsWith("Radio")).FirstOrDefault())?.Tag);
            }
        }
    }
}
