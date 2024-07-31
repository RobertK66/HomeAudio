using AudioCollectionApi.api;
using AudioCollectionApi.model;
using AvaloniaHomeAudio.player;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AvaloniaHomeAudio.ViewModels;


public static class TestData {
    public static MainViewModel TestInstance = new MainViewModel(null, null, null);
}


public partial class MainViewModel : ViewModelBase
{
    private readonly ILogger? _logger;
    private readonly IMediaRepository? _repos;
    private readonly IChromeCastPlayer _player;

    [ObservableProperty]
    public string _Greeting = "Welcome to MyHomeAudio!";

    [ObservableProperty]
    public ObservableCollection<MediaCategory> _categories = new ObservableCollection<MediaCategory>();

    [ObservableProperty]
    public ObservableCollection<IMedia> _mediaList = new ObservableCollection<IMedia>();

    public MainViewModel(ILogger<MainViewModel>? logger, IMediaRepository? repos, IChromeCastPlayer? player) {
        _logger = logger;
        _repos = repos;
        _player = player;

        _logger?.LogInformation("************************************* Constructor called *****************************************");
    }

    public async Task LoadReposAsync() {
        if (_repos != null) {
            await _repos.LoadAllAsync("./data");
            Categories = _repos.GetCategories();
        } else {
            await Task.CompletedTask;
        }
    }

    public void SelectCategory(string cat) {
        if (_repos != null) {
            MediaList = _repos.GetMediaRepository(cat);
        }
    }

    internal void PlayMedia(IMedia media) {
        _player?.Play(media);
    }
}
