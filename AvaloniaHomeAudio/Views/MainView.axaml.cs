using Avalonia.Controls;
using AvaloniaHomeAudio.ViewModels;

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
}
