using AudioCollectionApi.api;
using AudioCollectionApi.model;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AvaloniaHomeAudio.ViewModels;


public static class TestData {
    public static MainViewModel TestInstance = new MainViewModel(null, null);
}


public partial class MainViewModel : ViewModelBase
{
    private readonly ILogger? _logger;
    private readonly IMediaRepository? _repos;

    [ObservableProperty]
    public string _Greeting = "Welcome to MyHomeAudio!";

    [ObservableProperty]
    public ObservableCollection<MediaCategory> _categories = new ObservableCollection<MediaCategory>();
         
    public MainViewModel(ILogger<MainViewModel>? logger, IMediaRepository? repos) {
        _logger = logger;
        _repos = repos;
        _logger?.LogInformation("************************************* Constructor called *****************************************");
    }

    internal async Task LoadReposAsync() {
        if (_repos != null) {
            await _repos.LoadAllAsync("./data");
            Categories = _repos.GetCategories();
        } else {
            await Task.CompletedTask;
        }
    }
}
