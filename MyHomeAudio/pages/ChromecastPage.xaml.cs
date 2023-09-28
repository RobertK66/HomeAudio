using ABI.Windows.Data.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyHomeAudio.logger;
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

        public LoggerVm LoggerVm { get; set; }

        private ChromeCastClientWrapper _selectedCCC;
        public ChromeCastClientWrapper SelectedCcc { get { return _selectedCCC; } set { if (_selectedCCC != value) { _selectedCCC = value; RaisePropertyChanged(); } } }


        private ObservableCollection<ChromeCastClientWrapper> _ccc = null;
        public ObservableCollection<ChromeCastClientWrapper> CCC {
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

            CCC = App.Current.ChromeCastRepos.GetClients();
            if (CCC.Count > 0) {
                SelectedCcc = App.Current.m_window.ActiveCcc;
                //SelectedCcc = CCC.Where(cc=>cc.Name.StartsWith("Bü")).FirstOrDefault()??CCC[0];
                //App.Current.ChromeCastRepos.SetActiveClient(SelectedCcc);
                //_ = SelectedCcc.TryConnectAsync();
            }

            LoggerVm = (LoggerVm) App.Current.MyHost.Services.GetService(typeof(LoggerVm));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {

        }

        private void ChromeCastClients_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //App.Current.m_window.ActiveCcc = SelectedCcc;
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e) {
            logScroll.ScrollToVerticalOffset(logScroll.ScrollableHeight);
        }
    }
}
