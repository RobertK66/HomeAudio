using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUiHomeAudio.model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ObservableCollection<Category> Categories = new();
        public ObservableCollection<Category> FooterCategories = new();


        //private ChromeCastClientWrapper? _selectedCCC;
        //public ChromeCastClientWrapper? ActiveCcc { get { return _selectedCCC; } set { if (_selectedCCC != value) { _selectedCCC = value; RaisePropertyChanged(); } } }

        private ChromeCastRepository _ccRepos;
        public ChromeCastRepository ccRepos { get { return _ccRepos; } set { if (_ccRepos != value) { _ccRepos = value; RaisePropertyChanged(); } } }



        public MainPage() {
            this.InitializeComponent();

            ccRepos = (App.Current as App).MyHost.Services.GetRequiredService<ChromeCastRepository>();

        }


        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            //if (args.IsSettingsInvoked) {
            //    //this.ccPlayer.Visibility = Visibility.Collapsed;
            //    ContentFrame.Navigate(typeof(SettingsPage));
            //} else {
            //    //this.ccPlayer.Visibility = Visibility.Visible;
            //    string selectedItem = (String)args.InvokedItem;
            //    if (selectedItem.Equals("ChromeCast")) {
            //        ContentFrame.Navigate(typeof(ChromecastPage));
            //    } else if (selectedItem.Contains("Cd")) {
            //        ContentFrame.Navigate(typeof(CdPage), args.InvokedItemContainer.Tag.ToString());
            //    } else {
            //        ContentFrame.Navigate(typeof(RadioPage), args.InvokedItemContainer.Tag.ToString());
            //    }
            //}
        }

        private void CcPlayer_VolumeUp(object sender, EventArgs e) {
            if (sender is CcPlayer p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    ccw.VolumeUp();
                }
            }

        }

        private void CcPlayer_VolumeDown(object sender, EventArgs e) {
            if (sender is CcPlayer p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    ccw.VolumeDown();
                }
            }
        }

        private void ccPlayer_ConnectToggeled(object sender, EventArgs e) {
            if (sender is CcPlayer p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    if (p.IsConnected) {
                        //_ = ; Todo disconnect
                    } else {
                        _ = ccw.TryConnectAsync(ccRepos._appId);
                    }

                 
                }
            }
        }
    }
}
