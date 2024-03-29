using CommunityToolkit.Labs.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using Microsoft.WindowsAppSDK.Runtime;
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
using static System.Net.Mime.MediaTypeNames;
using WASDK = Microsoft.WindowsAppSDK;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged {

        private static int loadcount = 0;
        private static int constcount = 0;
        //private static int navigatecount = 0;
        private TimeSpan? loadTime = null;

        public string EaVersion {
            get {
                var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public string EaName {
            get {
                return System.Reflection.Assembly.GetEntryAssembly().FullName;
            }
        }

        public string EaDecription {
            get {
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            }
        }

        public string PackageVersion {
            get {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("{0}.{1}.{2}.{3} ", version.Major, version.Minor, version.Build, version.Revision);
                                                                    
            }
        }

        private String _sts ="<not set>";
        public String SettingsTitleString {
            get {
                return _sts;
            }

            set {
                if (_sts != value) {
                    _sts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _repospath = ApplicationData.Current.LocalFolder.Path;
        public String RepositoryPath {
            get {
                return _repospath;
            }

            set {
                if (_repospath != value) {
                    _repospath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int ListReposFiles() {
            int i = 0;
            try {
                RepositoryFiles.Clear();
                foreach (var s in Directory.GetFiles(RepositoryPath, "*.json")) {
                    RepositoryFiles.Add(s);
                    i++;
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
            return i;
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


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string? propertyName = null) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string WinAppSdkDetails {
            get => App.WinAppSdkDetails;
        }

        public string WinAppSdkRuntimeDetails {
            get => App.WinAppSdkRuntimeDetails;
        }

        public SettingsPage() {
            DateTime startTime = DateTime.Now;

            this.InitializeComponent();

            foreach (var a in System.Reflection.Assembly.GetEntryAssembly()?.GetReferencedAssemblies()) {
                try {
                    var asm = Assembly.Load(a);
                    this.VersionExpander.Items.Add(new SettingsCard() {
                        Header = a.FullName,
                        Content = string.Format("{0}.{1}.{2}.{3} ", a.Version.Major, a.Version.Minor, a.Version.Build, a.Version.Revision),
                        Description = asm.Location
                    });
                } catch { }
            }

            //var c = ApplicationData.Current;

            //this.VersionExpander.Items.Add(new SettingsCard() {
            //    Header = c.LocalFolder?.Path,
            //    Content = c.RoamingFolder?.Path,
            //    Description = c.SharedLocalFolder?.Path
            //});

            loadTime = DateTime.Now - startTime;
            constcount++;
            SettingsTitleString = string.Format("Settings Page constructed: {0}, loaded: {1} time: {2} ms.", constcount, loadcount, loadTime.Value.TotalMilliseconds);

            var configPath = ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath]?.ToString();
            if (configPath != null) {
                RepositoryPath = configPath;
            } 
        }

        private void themeMode_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();
            if (selectedTheme != null) {
                var t = App.GetEnum<ElementTheme>(selectedTheme);

                if (App.Current.m_window.Content is FrameworkElement rootElement) {
                    rootElement.RequestedTheme = t;
                    ApplicationData.Current.LocalSettings.Values[AppSettingKeys.UiTheme] = t.ToString();
                }
            }
         
        }

        private void navigationLocation_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {
            NavigationView navPane = App.Current.m_window.MainNavPane;

            if (navigationLocation.SelectedIndex == 0) {
                navPane.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                ApplicationData.Current.LocalSettings.Values[AppSettingKeys.IsLeftMode] = true;
            } else {
                navPane.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                ApplicationData.Current.LocalSettings.Values[AppSettingKeys.IsLeftMode] = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            loadcount++;
            SettingsTitleString = string.Format("Settings Page constructed: {0}, loaded: {1} time: {2} ms.", constcount, loadcount, loadTime.Value.TotalMilliseconds);

            var isLeft = ApplicationData.Current.LocalSettings.Values[AppSettingKeys.IsLeftMode];
            if (isLeft == null || ((bool)isLeft == true)) {
                navigationLocation.SelectedIndex = 0;
            } else {
                navigationLocation.SelectedIndex = 1;
            }

            ElementTheme currentTheme = ElementTheme.Light;
            if (App.Current.m_window.Content is FrameworkElement rootElement) {
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
            if (ListReposFiles()>0) {
                String configPath = ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath]?.ToString();
                if (configPath != null) {
                    if (!RepositoryPath.Equals(configPath)) {
                        // it really changend and there are valid files!
                        ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath] = RepositoryPath;
                        App.Current.ReconfigureMainWindow(RepositoryPath);
                    }
                } else {
                    ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath] = RepositoryPath;
                    App.Current.ReconfigureMainWindow(RepositoryPath);
                }
            }
        }

        private void reposPath_TextChanged(object sender, TextChangedEventArgs e) {
            fileExpander.IsExpanded = false;        //TODO: find out why this is needed Observable collection should work as items if expanden !?
            ListReposFiles();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            RepositoryPath = ApplicationData.Current.LocalFolder.Path;
        }

        
    }
}
