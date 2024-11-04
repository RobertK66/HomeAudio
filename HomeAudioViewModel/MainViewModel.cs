﻿using AudioCollectionApi.api;
using AudioCollectionApi.model;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace HomeAudioViewModel;


public partial class MainViewModel : ViewModelBase {
    private readonly ILogger? _logger;
    private readonly IMediaRepository? _repos;
    private readonly IPlayerRepository _playerRepos;


    //[ObservableProperty]
    private IPlayerProxy? _selectedPlayer;
    public IPlayerProxy? SelectedPlayer {
        get { return _selectedPlayer; }
        set { SetProperty(ref _selectedPlayer, value);
              _playerRepos?.SetActiveClient(value);
        }
    }

    [ObservableProperty]
    public ObservableCollection<IPlayerProxy> _knownPlayers = new ObservableCollection<IPlayerProxy>();

    [ObservableProperty]
    public string _Greeting = "Welcome to MyHomeAudio!";

    [ObservableProperty]
    public ObservableCollection<MediaCategory> _categories = new ObservableCollection<MediaCategory>();

    [ObservableProperty]
    public ObservableCollection<IMedia> _mediaList = new ObservableCollection<IMedia>();

    public MainViewModel(ILogger<MainViewModel>? logger, IMediaRepository? repos, IPlayerRepository playerRepos) {
        _logger = logger;
        _repos = repos;
        //        _player = player;
        _playerRepos = playerRepos;
    }

    public virtual async Task LoadReposAsync() {
        if (_repos != null) {
            _logger?.LogInformation("************************************* LoadRepos called *****************************************");
          
            int i = 0;
            //await Task.Delay(5000);
            await _repos.LoadAllAsync("");

            //List<Task> tasks = new List<Task>();
            //List<Stream> streams = new List<Stream>();
            //foreach (var uri in AssetLoader.GetAssets(new("avares://AvaloniaHomeAudio/Assets"), null)) {
            //    if ((uri != null) && uri.AbsolutePath.EndsWith(".json")) {
            //        //   using (Stream stream = new BufferedStream(AssetLoader.Open(uri))) {  // This does not work in android -> closed stream is used at 2nd call!?
            //        Stream stream = AssetLoader.Open(uri);
            //        streams.Add(stream);
            //        tasks.Add(_repos.LoadReposAsync(uri.Segments[2].Replace(".json", ""), stream));
            //        //   }
            //    }
            //}
            //await Task.WhenAll(tasks);
            //// now close all streams
            //foreach (var stream in streams) {
            //    _ = stream.DisposeAsync();
            //}

            _logger?.LogInformation("************************************* LoadRepos ready *****************************************");
            Categories = _repos.GetCategories();
        }

        if (_playerRepos != null) {
            _playerRepos.PlayerFound += _playerRepos_PlayerFound; ;
            _ = _playerRepos.LoadAllAsync();
        }

    }

    private void _playerRepos_PlayerFound(object? sender, IPlayerProxy pp) {
        if (pp != null) {
            KnownPlayers.Add(pp);
            if (pp.Name.ToLower().StartsWith("loud")) {
                _logger?.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", pp.Name);

                pp.TryConnectAsync("");
                SelectedPlayer = pp;
                // _playerRepos.SetActiveClient(pp);
                //_ = _playerRepos.TryConnectAsync(pp);

                //if (m_window != null) {
                //    m_window.MainPage.SelectedChromecast = pp as IPlayerProxy;
                //}
            }
        }
    }

    public void SelectCategory(string cat) {
        if (_repos != null) {
            MediaList = _repos.GetMediaRepository(cat);
        }
    }

    internal void PlayMedia(IMedia media) {
        //Player?.Play(media);
        //if (media.IsCollection) {
        //    Player?.PlayCd(media);
        //} else {
        //    Player?.PlayRadio(media);
        //}
    }

    public void VolumeUp() {
        //_player
       //Player?.VolumeUp();
    }

    public void VolumeDown() {
        //Player?.VolumeDown();
    }
}

