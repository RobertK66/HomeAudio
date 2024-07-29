using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using WinUiHomeAudio.logger;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChromecastPage : VmPage {

        private LoggerVm? _loggerVm;
        public LoggerVm LoggerVm { get { return _loggerVm; } set { if (_loggerVm != value) { _loggerVm = value; RaisePropertyChanged(); } } }

        //private ChromeCastClientWrapper? _selectedCCC;
        //public ChromeCastClientWrapper? SelectedCcc { get { return _selectedCCC; } set { if (_selectedCCC != value) { _selectedCCC = value; RaisePropertyChanged(); } } }


        //private ObservableCollection<ChromeCastClientWrapper> _ccc = new();
        //public ObservableCollection<ChromeCastClientWrapper> CCC {
        //    get {
        //        return _ccc;
        //    }

        //    set {
        //        if (_ccc != value) {
        //            _ccc = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        public ChromecastPage() {
            this.InitializeComponent();

            //            CCC = App.Host.Services.GetRequiredService<ChromeCastRepository>().GetClients();
            //            if (CCC.Count > 0) {
            ////                SelectedCcc = App.Current.m_window?.ActiveCcc;
            //                //SelectedCcc = CCC.Where(cc=>cc.Name.StartsWith("Bü")).FirstOrDefault()??CCC[0];
            //                //App.Current.ChromeCastRepos.SetActiveClient(SelectedCcc);
            //                //_ = SelectedCcc.TryConnectAsync();
            //            }

            LoggerVm = (LoggerVm)App.Host.Services.GetRequiredService(typeof(LoggerVm));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {

        }

        private void ChromeCastClients_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //App.Current.m_window.ActiveCcc = SelectedCcc;
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e) {
            bool scrollit = NearlyEqual(e.PreviousSize.Height, logScroll.VerticalOffset + logScroll.RenderSize.Height, 0.001);

            //Debug.WriteLine("TB-H: " + e.PreviousSize.Height + " SA: " + (logScroll.VerticalOffset + logScroll.RenderSize.Height).ToString() + " -> Scroll It: " + scrollit);

            if (scrollit) {
                logScroll.ScrollToVerticalOffset(logScroll.ScrollableHeight);
            }

        }


        public static bool NearlyEqual(double a, double b, double epsilon) {
            const double MinNormal = 2.2250738585072014E-308d;
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a.Equals(b)) { // shortcut, handles infinities
                return true;
            } else if (a == 0 || b == 0 || absA + absB < MinNormal) {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * MinNormal);
            } else { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            LoggerVm.Clear();
        }
    }
}
