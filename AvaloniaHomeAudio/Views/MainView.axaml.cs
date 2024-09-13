using AudioCollectionApi.api;
using Avalonia.Controls;
using AvaloniaHomeAudio.ViewModels;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvaloniaHomeAudio.Views;

public partial class MainView : UserControl
{
    private MainViewModel vm { get { return DataContext as MainViewModel; } }

    public MainView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        vm.Greeting += "+";

    }

    private void UserControl_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        _ = vm.LoadReposAsync();
    }

    private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {

        string cat = (sender as Button)?.Tag as string ?? "";
        vm.SelectCategory(cat);
    }

    private void Button_Click_3(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        IMedia? media = (sender as Button)?.Tag as IMedia;
        if (media != null) {
            vm.PlayMedia(media);
        }
    }

}
