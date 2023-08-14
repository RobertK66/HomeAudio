using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page {

        public string WinAppSdkDetails {
            get => App.WinAppSdkDetails;
        }

        public string WinAppSdkRuntimeDetails {
            get {
                var details = WinAppSdkDetails;

                details += "WASDK.Runtime.Version.DotQuadString";
#if WindowsAppSdkRuntimeDependent
                details += ", Windows App Runtime " + WASDK.Runtime.Version.DotQuadString;
#endif
                return details;
            }
        }

        public SettingsPage() {
            this.InitializeComponent();
        }

        private void themeMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void navigationLocation_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
    }
}
