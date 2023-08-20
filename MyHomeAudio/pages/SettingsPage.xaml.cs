using CommunityToolkit.Labs.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.WindowsAppSDK.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Net.Mime.MediaTypeNames;
using WASDK = Microsoft.WindowsAppSDK;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page {

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

                return string.Format("{0}.{1}.{2}.{3} ",  version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public string WinAppSdkDetails {
            get => App.WinAppSdkDetails;
        }

        public string WinAppSdkRuntimeDetails {
            get => App.WinAppSdkRuntimeDetails;
        }

        public SettingsPage() {
            this.InitializeComponent();

            foreach(var a in System.Reflection.Assembly.GetEntryAssembly().GetReferencedAssemblies()) {
                var asm = Assembly.Load(a);
                this.VersionExpander.Items.Add(new SettingsCard() {
                    Header = a.FullName,
                    Content = string.Format("{0}.{1}.{2}.{3} ", a.Version.Major, a.Version.Minor, a.Version.Build, a.Version.Revision),
                    Description = asm.Location
                }); ;
            }
        }

        //< labs:SettingsCard Header = "Dll 1" >
        //                        < TextBlock Foreground = "{ThemeResource TextFillColorSecondaryBrush}" Text = "1,2,3,4" />
        //                    </ labs:SettingsCard >



        private void themeMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    }
    private void navigationLocation_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    }

        private void themeMode_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {

        }

        private void navigationLocation_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {

        }

        private void spatialSoundBox_Toggled(object sender, RoutedEventArgs e) {

        }

        private void screenshotModeToggle_Toggled(object sender, RoutedEventArgs e) {

        }

        private void screenshotFolderLink_Click(object sender, RoutedEventArgs e) {

        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {

        }
        private void soundToggle_Toggled(object sender, RoutedEventArgs e) {

        }

        private void bugRequestCard_Click(object sender, RoutedEventArgs e) {

        }
    }
}
