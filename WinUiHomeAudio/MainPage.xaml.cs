using AudioCollectionApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinUiHomeAudio.model;
using WinUiHomeAudio.pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Category> Categories = new();
        public ObservableCollection<Category> FooterCategories = new();

        private ChromeCastRepository _ccRepos;
        public ChromeCastRepository ccRepos { get { return _ccRepos; } set { if (_ccRepos != value) { _ccRepos = value; RaisePropertyChanged(); } } }


        private ChromeCastClientWrapper? _SelectedChromecast;
        public ChromeCastClientWrapper? SelectedChromecast { get { return _SelectedChromecast; } 
                                                             set { if (_SelectedChromecast != value) {
                                                                       _SelectedChromecast = value;
                                                                       RaisePropertyChanged();
                                                                       ccRepos.SetActiveClient(value);
                                                                   } 
                                                             } }

        public MainPage() {
            this.InitializeComponent();

            _ccRepos = App.Host.Services.GetRequiredService<ChromeCastRepository>();

            IEnumerable<IMediaRepository> mrs = App.Host.Services.GetServices<IMediaRepository>();
            foreach (IMediaRepository mr in mrs) {

                //            IMediaRepository mr = App.Host.Services.GetServices<IMediaRepository>();

                mr.GetCdCategories().CollectionChanged += (s, e) => {
                    if (e.NewItems != null) {
                        foreach (var ni in e.NewItems) {
                            if (ni is MediaCategory mc) {
                                Categories.Add(new Category() { Glyph = Symbol.Target, Name = mc.Name + "-Cds", Tag = mc.Id });
                            }
                        }
                    }
                };

                mr.GetRadioCategories().CollectionChanged += (s, e) => {
                    if (e.NewItems != null) {
                        foreach (var ni in e.NewItems) {
                            if (ni is MediaCategory mc) {
                                Categories.Add(new Category() { Glyph = Symbol.Account, Name = mc.Name ?? "unknown", Tag = mc.Id });
                            }
                        }
                    }
                };

                _ = mr.LoadAllAsync(ApplicationData.Current.LocalFolder.Path);
            }
        }


        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                //this.ccPlayer.Visibility = Visibility.Collapsed;
                //ContentFrame.Navigate(typeof(SettingsPage));
            } else {
                //this.ccPlayer.Visibility = Visibility.Visible;
                string selectedItem = (String)args.InvokedItem;
                if (selectedItem.Equals("ChromeCast")) {
                    //ContentFrame.Navigate(typeof(ChromecastPage));
                } else if (selectedItem.Contains("Cd")) {
                    ContentFrame.Navigate(typeof(CdPage), args.InvokedItemContainer.Tag.ToString());
                } else {
                    ContentFrame.Navigate(typeof(RadioPage), args.InvokedItemContainer.Tag.ToString());
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

        public void ccPlayer_ConnectToggeled(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    if (ccw.IsConnected != (e.OriginalSource as ToggleSwitch)?.IsOn) {
                        // This was because of user  toggle click
                        if (ccw.IsConnected) {
                            ccw.Disconnect();
                        } else {
                            _ = ccw.TryConnectAsync(ccRepos._appId);
                        }

                    }
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement p) {
                if (p.DataContext is ChromeCastClientWrapper ccw) {
                    ccw.StopMediaPlay();

                }
            }
        }
    }
}
