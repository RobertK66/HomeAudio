using ABI.Windows.Data.Json;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyHomeAudio.model;
using Sharpcaster.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChromecastPage: VmPage {

        private ChromeCastClient _selectedCCC;
        public ChromeCastClient SelectedCcc { get { return _selectedCCC; } set { if (_selectedCCC != value) { _selectedCCC = value; RaisePropertyChanged(); } } }


        private ObservableCollection<ChromeCastClient> _ccc = null;
        public ObservableCollection<ChromeCastClient> CCC {
            get {
                return _ccc;
            }

            set {
                if (_ccc != value) {
                    _ccc = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ChromecastPage() {
            this.InitializeComponent();
            var dq = DispatcherQueue.GetForCurrentThread();

            CCC = App.Current.ChromeCastRepos.GetClients();
            //foreach (var cr in App.Current.KnownChromecastReceiver) {
            //    CCC.Add(new ChromeCastClient(cr, dq));
            //}
            if (CCC.Count > 0) {
                SelectedCcc = CCC.Where(cc=>cc.Name.StartsWith("Bü")).FirstOrDefault()??CCC[0];
                App.Current.ChromeCastRepos.SetActiveClient(SelectedCcc);
                //_ = SelectedCcc.TryConnectAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            SelectedCcc.VolumeUp();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            SelectedCcc.VolumeDown();
        }
    }
}
