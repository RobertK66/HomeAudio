using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Avalonia.Platform;
using AvaloniaHomeAudio.player;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        _logger?.LogInformation("************************************* LoadRepos called *****************************************");
        if (_repos != null) {
            int i = 0;

            List<Task> tasks = new List<Task>();
            List<Stream> streams = new List<Stream>();
            foreach (var uri in AssetLoader.GetAssets(new("avares://AvaloniaHomeAudio/Assets"), null)) {
                if ((uri != null) && uri.AbsolutePath.EndsWith(".json")) {
                    //   using (Stream stream = new BufferedStream(AssetLoader.Open(uri))) {  // This does not work in android -> closed stream is used at 2nd call!?
                    Stream stream = AssetLoader.Open(uri);
                    streams.Add(stream);
                    tasks.Add(_repos.LoadReposAsync(uri.Segments[2].Replace(".json",""), stream));
                    //   }
                }
            }
            await Task.WhenAll(tasks);
            // now close all streams
            foreach(var stream in streams) {
                _ = stream.DisposeAsync();
            }
            
            _logger?.LogInformation("************************************* LoadRepos ready *****************************************");
            Categories = _repos.GetCategories();
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
