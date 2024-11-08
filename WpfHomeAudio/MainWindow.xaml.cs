using AudioCollectionApi.api;
using AudioCollectionApi.model;
using HomeAudioViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfHomeAudio {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        //private IObservableContext? _context;
        private object _lock = new();

        private MainViewModel _vm { get { return (MainViewModel)DataContext; } }

        public MainWindow(ILogger<MainWindow> logger, MainViewModel vm) {
            DataContext = vm;
            InitializeComponent();

            BindingOperations.EnableCollectionSynchronization(vm.KnownPlayers, _lock);

            // Make a real background thread for this to check async property/Collection Change events are synchronized.
            new Thread(() => _ = vm.LoadReposAsync()) { IsBackground = true }.Start();
            //_ = vm.LoadReposAsync();
        }

        //public MainWindow(ILogger<MainWindow> logger, MainViewModel vm, IObservableContext uiContext) : this(logger, vm) {
        //    _context = uiContext;
        //}

        // Change the Media Page(Category)
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var menuItem = e.NewValue as MediaCategory;

            if (menuItem != null) {
                _vm.SelectCategory(menuItem.Id);
            }
        }

        // Play clicked media        
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = (sender as ListView)?.SelectedItem as IMedia;

            if (item != null) {
                if (item.IsCollection) {
                    _vm.SelectedPlayer?.PlayCd(item);
                } else {
                    _vm.SelectedPlayer?.PlayRadio(item);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.VolumeUp();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.VolumeDown();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.Stop();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.Play();
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            _ = _vm.SelectedPlayer?.TryConnectAsync("");
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            _vm.SelectedPlayer?.Disconnect();
        }
    }
}