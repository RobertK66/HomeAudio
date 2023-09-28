using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyHomeAudio.logger;
using MyHomeAudio.model;
using Sharpcaster.Interfaces;
using Sharpcaster.Models;
using Sharpcaster.Models.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Microsoft.UI.Dispatching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Security;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio {

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {

        public ChromeCastRepository ChromeCastRepos = null;

        public IHost MyHost;
        private ILoggerFactory loggerFactory;
        private ILogger<App> Log;

        public App() {
            this.InitializeComponent();
            
            // Make Instance of Logger View Model here to give it the GUI Dispatcher queue.
            LoggerVm logVm = new LoggerVm();
            logVm.dq = DispatcherQueue.GetForCurrentThread();

            MyHost = Microsoft.Extensions.Hosting.Host.
                         CreateDefaultBuilder().
                         ConfigureServices((context, services) => {
                             services.AddSingleton(logVm);
                             services.AddSingleton<MediaRepository>();

                             services.AddLogging(logging => {
                                 logging.AddFilter(level => level >= LogLevel.Trace)
                                 .AddWinUiLogger((con) => {       // This adds our LogPanel as possible target (configure in appsettings.json)
                                     con.LoggerVm = logVm;
                                 });
                             });
                         }).
                         Build();

            Log = MyHost.Services.GetService<ILogger<App>>();
            loggerFactory = MyHost.Services.GetService<ILoggerFactory>();
            Log.LogTrace("Application instanciated.");
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {

            try {
                Log.LogInformation("Application loaded.");

                String appId = (string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AppId];
                if (String.IsNullOrEmpty(appId)) {
                    appId = AppSetting.DefaultAppId;
                }

                ChromeCastRepos = new ChromeCastRepository((string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AutoConnect],
                                                           appId, loggerFactory);

                String fullpath = System.Reflection.Assembly.GetEntryAssembly().Location;
                String path = fullpath.Substring(0, fullpath.LastIndexOf("\\"));

                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\Cds.json")) {
                    System.IO.File.Copy(path + "\\Cds.json", ApplicationData.Current.LocalFolder.Path + "\\Cds.json", true);
                }
                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json")) {
                    System.IO.File.Copy(path + "\\WebRadios.json", ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json");
                }

                IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
                locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
                _ = locator.FindReceiversAsync(CancellationToken.None);         // Fire the search process and wait for receiver found events in the handler. No await here!



            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            m_window = new MainWindow(); // { LoggerVm = (LoggerVm)App.Current.MyHost.Services.GetService(typeof(LoggerVm)) };
            //m_window.ExtendsContentIntoTitleBar = true;
            m_window.Activate();
        }

        private void Locator_ChromecastReceivedFound(object sender, Sharpcaster.Models.ChromecastReceiver e) {
            ChromeCastRepos.Add(e);
            //KnownChromecastReceiver.Add(e);
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
