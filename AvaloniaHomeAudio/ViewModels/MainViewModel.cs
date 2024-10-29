using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Avalonia.Controls;
using Avalonia.Platform;
using AvaloniaHomeAudio.player;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Sharpcaster.Models.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace AvaloniaHomeAudio.ViewModels;


public static class TestData {
    private class DummyPlayer : IPlayerProxy {

        private string _playerstatus;
        public string PlayerStatus { get { return _playerstatus; } }

        public string Name => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public string Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? MediaStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Play(IMedia media) {
        }

        public void VolumeDown() {
            //throw new NotImplementedException();
        }

        public void VolumeUp() {
            //throw new NotImplementedException();
        }

        public void Disconnect() {
            throw new NotImplementedException();
        }

        public Task<bool> TryConnectAsync(string appId) {
            throw new NotImplementedException();
        }

        public void PlayCd(IMedia cd) {
            throw new NotImplementedException();
        }

        public void PlayRadio(IMedia radio) {
            throw new NotImplementedException();
        }

        public void Stop() {
            throw new NotImplementedException();
        }

        public void Play() {
            throw new NotImplementedException();
        }

        public void SetContext<Object>(IObservableContext<Object> myContext) {
            throw new NotImplementedException();
        }

        public DummyPlayer(string text) {
            _playerstatus = text;
        }
    }

    private class DummyPlayerRepos : IPlayerRepository {

        ObservableCollection<IPlayerProxy> IPlayerRepository.KnownPlayer => getKnownPlayer();


        private ObservableCollection<IPlayerProxy> DesignPlayer = new ObservableCollection<IPlayerProxy>();
        private ObservableCollection<IPlayerProxy> getKnownPlayer() {
            if (DesignPlayer.Count == 0) {
                DesignPlayer.Add(new DummyPlayer("First TestPlayer"));
                DesignPlayer.Add(new DummyPlayer("2nd TestPlayer"));
            }

            return DesignPlayer;            
        }

        public event EventHandler<IPlayerProxy> PlayerFound;

        public Task LoadAllAsync() {
            throw new NotImplementedException();
        }

        public void SetActiveClient(IPlayerProxy? value) {
            throw new NotImplementedException();
        }

        public Task TryConnectAsync(IPlayerProxy ccw) {
            throw new NotImplementedException();
        }
    }


    public static MainViewModel TestInstance = new MainViewModel(null, null, new DummyPlayerRepos()); // new DummyPlayer("Design Time Player"));
}


public partial class MainViewModel : ViewModelBase
{
    private readonly ILogger? _logger;
    private readonly IMediaRepository? _repos;
    private readonly IPlayerRepository _playerRepos;
    


    [ObservableProperty]
    public IPlayerProxy? _player;

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
            if (pp.Name.ToLower().StartsWith("loud")) {
                _logger?.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", pp.Name);

                pp.TryConnectAsync("");
                Player = pp;
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
        if (media.IsCollection) {
            Player?.PlayCd(media);
        } else {
            Player?.PlayRadio(media);
        }
    }

    public void VolumeUp() {
        //_player
        Player?.VolumeUp();
    }

    public void VolumeDown() {
        Player?.VolumeDown();
    }
}

 

