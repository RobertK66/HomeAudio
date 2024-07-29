
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using Sharpcaster.Models.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using WinUiHomeAudio.model;
using static System.Net.Mime.MediaTypeNames;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : VmPage {

        private ILogger Log;
        private AppSettings Settings;


        private static int loadcount = 0;
        private static int constcount = 0;
        //private static int navigatecount = 0;
        private TimeSpan? loadTime = null;


        public string PackageVersion {
            get {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("{0}.{1}.{2}.{3} ", version.Major, version.Minor, version.Build, version.Revision);

            }
        }


        public String RepositoryPath {
            get {
                return Settings.ReposPath;            }

            set {
                if (!Object.Equals(Settings.ReposPath, value)) {
                    Settings.ReposPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public String? AutoConnectName {
            get {
                return Settings.AutoConnectName;
            }

            set {
                if (Settings.AutoConnectName != value) {
                    Settings.AutoConnectName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public String UsedAppId {
            get {
                return Settings.AppId;
            }

            set {
                if (Settings.AppId != value) {
                    Settings.AppId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<String> _reposfiles = new ObservableCollection<String>();
        public ObservableCollection<String> RepositoryFiles {
            get {
                return _reposfiles;
            }

            set {
                if (_reposfiles != value) {
                    _reposfiles = value;
                    RaisePropertyChanged();
                }
            }
        }


        //public string WinAppSdkDetails {
        //    get => "App.WinAppSdkDetails;";
        //}

        //public string WinAppSdkRuntimeDetails {
        //    get => "App.WinAppSdkRuntimeDetails;";
        //}

        public SettingsPage() {
            DateTime startTime = DateTime.Now;
            Log = App.Host.Services.GetRequiredService<ILogger<SettingsPage>>();
            Settings = App.Host.Services.GetRequiredService<AppSettings>();

            this.InitializeComponent();

            var aa = Assembly.GetEntryAssembly()?.GetReferencedAssemblies();
            if (aa != null) {
                foreach (var a in aa.OrderBy(y=>y.Name)) {
                    try {
                        if (a != null) {
                            var asm = Assembly.Load(a);
                            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location);
                            this.VersionExpander.Items.Add(new SettingsCard() {
                                Header = fvi.InternalName??fvi.FileName,
                                Content = fvi.FileVersion, //string.Format("{0}.{1}.{2}.{3} ", a.Version?.Major, a.Version?.Minor, a.Version?.Build, a.Version?.Revision),
                                Description = a.FullName
                            }); 
                        }
                    } catch { }
                }
            }

            loadTime = DateTime.Now - startTime;
            constcount++;

            CreateFileList();
            Log.LogDebug(string.Format("Settings Page constructed: {0}, loaded: {1} time: {2} ms.", constcount, loadcount, loadTime.Value.TotalMilliseconds));
        }

        private int ListReposFiles() {
            int i = 0;
            try {
                RepositoryFiles.Clear();
                foreach (var s in Directory.GetFiles(RepositoryPath, "*.json")) {
                    RepositoryFiles.Add(Path.GetFullPath(s));
                    i++;
                }
            } catch (Exception ex) {
                Log.LogError("Exception reading repos files {ex}", ex);
            }
            return i;
        }




        private void themeMode_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();
            if (selectedTheme != null) {
                var t = Settings.GetEnum<ElementTheme>(selectedTheme);

                if ((App.Current as WinUiHomeAudio.App)?.m_window?.Content is FrameworkElement rootElement) {
                    rootElement.RequestedTheme = t;
                    Settings.UiTheme  = t.ToString();
                }
            }

        }

        private void navigationLocation_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {
            NavigationView? navPane = ((App.Current as WinUiHomeAudio.App)?.m_window?.MainPage as WinUiHomeAudio.MainPage)?.MainNavPane as NavigationView;
            if (navPane != null) {
                if (navigationLocation.SelectedIndex == 0) {
                    navPane.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                    Settings.IsLeftMode = true;
                } else {
                    navPane.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                    Settings.IsLeftMode = false;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            loadcount++;
            Log.LogDebug(string.Format("Settings Page constructed: {0}, loaded: {1} ", constcount, loadcount));

            if (Settings.IsLeftMode) {
                navigationLocation.SelectedIndex = 0;
            } else {
                navigationLocation.SelectedIndex = 1;
            }

            ElementTheme currentTheme = ElementTheme.Light;
            if ((App.Current as WinUiHomeAudio.App)?.m_window?.Content is FrameworkElement rootElement) {
                currentTheme = rootElement.RequestedTheme;
            }
            switch (currentTheme) {
                case ElementTheme.Light:
                    themeMode.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    themeMode.SelectedIndex = 1;
                    break;
                case ElementTheme.Default:
                    themeMode.SelectedIndex = 2;
                    break;
            }
        }

        private void SettingsExpander_Expanded(object sender, EventArgs e) {

        }

        private void reposPath_TextChanged(object sender, TextChangedEventArgs e) {
            CreateFileList();
        }

        private void CreateFileList() {
            if (ListReposFiles() > 0) {
                (App.Current as WinUiHomeAudio.App)?.m_window?.MainPage?.ReconfigureMediaFolder(RepositoryPath);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            RepositoryPath = ApplicationData.Current.LocalFolder.Path;
            CreateFileList();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e) {
            // Create a folder picker
            FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle((App.Current as WinUiHomeAudio.App)?.m_window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null) {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                RepositoryPath = folder.Path;
            }
        }

    }
}
