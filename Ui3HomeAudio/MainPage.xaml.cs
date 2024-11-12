using AudioCollectionApi.api;
using AudioCollectionApi.model;
using HomeAudioViewModel;
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
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Ui3HomeAudio {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {

        private MainViewModel _vm { get { return (MainViewModel)DataContext; } }

   
        public MainPage(ILogger<MainPage> _log, MainViewModel vm) {
            this.DataContext = vm;
            this.InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            _ = _vm.SelectedPlayer?.TryConnectAsync("");
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            _ = _vm.SelectedPlayer?.DisconnectAsync();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var menuItem = e.AddedItems[0] as MediaCategory;

            if (menuItem != null) {
                _vm.SelectCategory(menuItem.Id);
            }
        }

        private void ListBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            var item = (sender as ListBox)?.SelectedItem as IMedia;

            if (item != null) {
                if (item.IsCollection) {
                    _vm.SelectedPlayer?.PlayCd(item);
                } else {
                    _vm.SelectedPlayer?.PlayRadio(item);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.VolumeDown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.VolumeUp();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.Stop();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.Play();
        }
    }
}
