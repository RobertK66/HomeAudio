using AudioCollectionApi.api;
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
    private readonly IObservableContext? _context;
    private readonly IMediaRepository? _repos;
    private readonly IPlayerRepository _playerRepos;

    //[ObservableProperty]
    private IPlayerProxy? _selectedPlayer;
    public IPlayerProxy? SelectedPlayer {
        get { return _selectedPlayer; }
        set { SetProperty(ref _selectedPlayer, value);
              //_playerRepos?.SetActiveClient(value);
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


    // Constructor Option without IObservableContext for GUI frameworks that can handle/synchronize the IPropertyChanged on their own.
    public MainViewModel(ILogger<MainViewModel>? logger, IMediaRepository? repos, IPlayerRepository playerRepos) {
        _logger = logger;
        _repos = repos;
        _playerRepos = playerRepos;
    }

    // Environments which needs to post the IPropertyChanged/ICollectionChanged events on a different thread need an abstract Context for that!
    public MainViewModel(ILogger<MainViewModel>? logger, IMediaRepository? repos, IPlayerRepository playerRepos, IObservableContext uiContext) {
        _logger = logger;
        _repos = repos;
        _context = uiContext;   
        _playerRepos = playerRepos;
    }

    public virtual async Task LoadReposAsync() {
        if (_repos != null) {
            _logger?.LogInformation("************************************* LoadRepos called *****************************************");
         
            int i = 0;
            //await Task.Delay(5000);
            await _repos.LoadAllAsync("");

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
            if (_context != null) {
                pp.SetContext(_context);
            }
            KnownPlayers.Add(pp);
            if (pp.Name.ToLower().StartsWith("loud")) {
                _logger?.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", pp.Name);

                pp.TryConnectAsync("");
                SelectedPlayer = pp;
            }
        }
    }

    public void SelectCategory(string cat) {
        if (_repos != null) {
            MediaList = _repos.GetMediaRepository(cat);
        }
    }
    
}

