using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyHomeAudio.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {

        public MediaRepository MediaRepository = new MediaRepository();


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {

            try {
                String fullpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                String path = fullpath.Substring(0, fullpath.LastIndexOf("\\"));

                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\Cds.json")) {
                    System.IO.File.Copy(path + "\\Cds.json", ApplicationData.Current.LocalFolder.Path + "\\Cds.json");
                }
                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json")) {
                    System.IO.File.Copy(path + "\\WebRadios.json", ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json");
                }

            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            m_window = new MainWindow();
            //m_window.ExtendsContentIntoTitleBar = true;
            m_window.Activate();


        }

        public MainWindow m_window;


        public new static App Current => (App)Application.Current;

        public static string WinAppSdkDetails {
            get => "?? ?? ?? ??";
            //WASDK.Release.Major, WASDK.Release.Minor, WASDK.Release.Patch, WASDK.Release.VersionTag);
        }

        public static string WinAppSdkRuntimeDetails {
            get {
                var details = WinAppSdkDetails + "WASDK.Runtime.Version.DotQuadString";
                return details;
            }
        }

        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct {
            if (!typeof(TEnum).GetTypeInfo().IsEnum) {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

        internal void ReconfigureMainWindow(string repositoryPath) {
            m_window.BuildMenue(repositoryPath);

        }

        
    }
}
