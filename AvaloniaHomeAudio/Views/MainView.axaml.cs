using AudioCollectionApi.api;
using Avalonia.Controls;

using HomeAudioViewModel;
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

    //private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
    //    vm.Greeting += "+";

    //}

    private void UserControl_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
       //_ = vm.StartAsync(new System.Threading.CancellationToken());
    }

    private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {

        string cat = (sender as Button)?.Tag as string ?? "";
        vm.SelectCategory(cat);
    }

    private void Button_Click_3(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        IMedia? media = (sender as Button)?.Tag as IMedia;
        if (media != null) {
            if (media.IsCollection) {
                vm.SelectedPlayer?.PlayCd(media);
            } else {
                vm.SelectedPlayer?.PlayRadio(media);
            }
        }
    }

    private void Button_Click_Up(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        vm.SelectedPlayer?.VolumeUp();
    }

    private void Button_Click_Down(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        vm.SelectedPlayer?.VolumeDown();
    }

    private void Button_Click_Stop(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        vm.SelectedPlayer?.Stop();
    }
    private void Button_Click_Play(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        vm.SelectedPlayer?.Play();
    }
    
}
