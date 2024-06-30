using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaHomeAudio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public string _Greeting = "Welcome to MyHomeAudio!";

  
}
