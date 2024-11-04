using AudioCollectionApi.api;
using AudioCollectionApi.model;
using HomeAudioViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfHomeAudio {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
    
        public MainWindow(MainViewModel vm) {
            InitializeComponent();
            DataContext = vm; // WpfHomeAudio.App.ServiceProvider.GetService<MainViewModel>();
           _ = vm.LoadReposAsync();
           

        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var menuItem = e.NewValue as MediaCategory;
            if (menuItem != null) {
                (DataContext as MainViewModel)?.SelectCategory(menuItem.Id);
            }

        }
        
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = (sender as ListView)?.SelectedItem as IMedia;

            if (item != null) {
                if (item.IsCollection) {
                    (DataContext as MainViewModel)?.SelectedPlayer?.PlayCd(item);
                } else {
                    (DataContext as MainViewModel)?.SelectedPlayer?.PlayRadio(item);
                }
            }
        }

       
    }
}