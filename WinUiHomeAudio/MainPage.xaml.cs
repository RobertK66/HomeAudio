using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinUiHomeAudio.model;
using WinUiHomeAudio.pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged {

        private string ConfiguredAppId;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Category> Categories = [];
        public ObservableCollection<Category> FooterCategories = [];

        private IPlayerRepository _ccRepos;
        public IPlayerRepository CcRepos { get { return _ccRepos; } set { if (_ccRepos != value) { _ccRepos = value; RaisePropertyChanged(); } } }


        private IPlayerProxy? _SelectedChromecast;
        public IPlayerProxy? SelectedChromecast {
            get { return _SelectedChromecast; }
            set {
                if (_SelectedChromecast != value) {
                    _SelectedChromecast = value;
                    RaisePropertyChanged();
                    CcRepos.SetActiveClient(value);
                }
            }
        }

        public NavigationView MainNavPane { get => this.MainNavView; }


        public MainPage() {
            this.InitializeComponent();

            _ccRepos = App.Host.Services.GetRequiredService<IPlayerRepository>();
            FooterCategories.Add(new Category() { Name = "Live Logger", Tag = "Logger" });


            var settings = App.Host.Services.GetRequiredService<AppSettings>();
            IEnumerable<IMediaRepository> mrs = App.Host.Services.GetServices<IMediaRepository>();

            ConfiguredAppId = settings.AppId;

            foreach (IMediaRepository mr in mrs) {
                var cats = mr.GetCategories();
                foreach (var c in cats) {
                    Categories.Add(new Category() { Glyph = Symbol.Target, Name = c.Name, Tag = c.Id });
                }
                cats.CollectionChanged += (s, e) => {
                    if (e.NewItems != null) {
                        foreach (var ni in e.NewItems) {
                            if (ni is MediaCategory mc) {
                                Categories.Add(new Category() { Glyph = Symbol.Target, Name = mc.Name, Tag = mc.Id });
                            }
                        }
                    }
                };

                _ = mr.LoadAllAsync(settings.ReposPath);


            }
        }


        public void ReconfigureMediaFolder(string reposRootPath) {
            Categories.Clear();
            IEnumerable<IMediaRepository> mrs = App.Host.Services.GetServices<IMediaRepository>();
            foreach (IMediaRepository mr in mrs) {
                _ = mr.LoadAllAsync(reposRootPath);
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                //this.ccPlayer.Visibility = Visibility.Collapsed;
                ContentFrame.Navigate(typeof(SettingsPage));
            } else {
                var tag = args.InvokedItemContainer.Tag.ToString() ?? "??";
                if (tag.Equals("Logger")) {
                    ContentFrame.Navigate(typeof(LoggerPage));
                } else {
                    ContentFrame.Navigate(typeof(MediaPage), tag);
                }
            }
        }

        public void CcPlayer_VolumeUp(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    ccw.VolumeUp();
                }
            }
        }

        public void CcPlayer_VolumeDown(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    ccw.VolumeDown();
                }
            }
        }

        public void CcPlayer_ConnectToggeled(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    if (ccw.IsConnected != (e.OriginalSource as ToggleSwitch)?.IsOn) {
                        // This was because of user  toggle click
                        if (ccw.IsConnected) {
                            ccw.Disconnect();
                        } else {
                            _ = ccw.TryConnectAsync(ConfiguredAppId);
                        }
                    }
                }
            }
        }

        private async void Stop_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    await ccw.StopMediaPlay();
                }
            }
        }
    }
}
