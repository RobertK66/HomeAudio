using AudioCollectionApi.api;
using AudioCollectionImpl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Sharpcaster;
using System;
using System.IO;
using System.Threading;
using Windows.Storage;
using WinUiHomeAudio.logger;
using WinUiHomeAudio.model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio {



    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {

        public static readonly IHost Host = ((App)App.Current).MyHost;

        public readonly IHost MyHost;
        private readonly ILogger<App> Log;

        public MainWindow? m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();

            // Make Instance of Logger View Model here to pass it a reference to the GUI Dispatcher queue.
            var logVm = new LoggerVm {
                Dq = DispatcherQueue.GetForCurrentThread()
            };

            MyHost = Microsoft.Extensions.Hosting.Host.
                       CreateDefaultBuilder().
                       //ConfigureHttpJsonOptions(options => {
                       //    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                       //}).
                       ConfigureServices((context, services) => {
                           services.AddSingleton(logVm);
                           services.AddSingleton<IMediaRepository, JsonMediaRepository>();
                           //services.AddSingleton<IMediaRepository, DLNAAlbumRepository>();
                           services.AddSingleton<AppSettings>();
                           services.AddSingleton<ChromeCastRepository>();
                           services.AddLogging(logging => {
                                               logging.AddFilter(level => level >= LogLevel.Trace)
                                                      .AddWinUiLogger((con) => {    
                                                          // This adds our LogPanel as possible target (configure in appsettings.json)
                                                          con.LoggerVm = logVm;
                                                      });
                           });
                       }).
                       Build();

            Log = MyHost.Services.GetRequiredService<ILogger<App>>();
            Log.LogTrace("*** Application instanciated. ***");

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {

            // if not done already, copy the provided examples to current executable path. 
            //string? fullpath = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            if (System.Reflection.Assembly.GetEntryAssembly()?.Location is string fullpath) {
                string path = fullpath[..fullpath.LastIndexOf('\\')];

                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\Cds.json")) {
                    System.IO.File.Copy(path + "\\Cds.json", ApplicationData.Current.LocalFolder.Path + "\\Cds.json", true);
                }
                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json")) {
                    System.IO.File.Copy(path + "\\WebRadios.json", ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json");
                }
            }


            try {
                // start the search for CC Receivers in local network
                MdnsChromecastLocator locator = new();
                locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
                _ = locator.FindReceiversAsync(CancellationToken.None);          // Fire the search process and wait for receiver found events in the handler. No await here!
            } catch (Exception ex) {
                Log.LogError("Exception on Loading: {ex} ", ex);
            }

            m_window = new MainWindow();
            m_window.Activate();
        }

        private void Locator_ChromecastReceivedFound(object? sender, Sharpcaster.Models.ChromecastReceiver e) {
            var ccr = MyHost.Services.GetRequiredService<ChromeCastRepository>();
            var appSettings = MyHost.Services.GetRequiredService<AppSettings>();

            var ccc = ccr.Add(e);

            if (!String.IsNullOrEmpty(appSettings.AutoConnectName) && e.Name.StartsWith(appSettings.AutoConnectName)) {
                Log.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", e.Name);
                _ = ccr.TryConnectAsync(ccc);

                if (m_window != null) {
                    m_window.MainPage.SelectedChromecast = ccc;
                }

            }

        }

    }
}
