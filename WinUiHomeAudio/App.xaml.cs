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
                           // TODO: read log levels from config ...
                           services.AddLogging(logging => {
                                               logging.AddFilter(level => level >= LogLevel.Trace)
                                                      .AddWinUiLogger((con) => {    
                                                          // This adds our LogPanel as possible target (configure in appsettings.json)
                                                          con.LoggerVm = logVm;
                                                      });
                           });
                       }).
                       Build();

            // no need to start this host - We only use DI and have no 'services' to start.
            // MyHost.Start();

            Log = MyHost.Services.GetRequiredService<ILogger<App>>();
            Log.LogTrace("*** Application instanciated. ***");

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {

            try {
                // copy the provided examples to current executable path. 
                if (System.Reflection.Assembly.GetEntryAssembly()?.Location is string fullpath) {
                    string path = fullpath[..fullpath.LastIndexOf('\\')];
                    System.IO.File.Copy(path + "\\Cds.json", ApplicationData.Current.LocalFolder.Path + "\\Cds.json", true);
                    System.IO.File.Copy(path + "\\WebRadios.json", ApplicationData.Current.LocalFolder.Path + "\\WebRadios.json", true);
                }

                // start the search for CC Receivers in local network
                MdnsChromecastLocator locator = new();
                locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
                CancellationTokenSource tokenSource = new CancellationTokenSource(10000);
                _ = locator.FindReceiversAsync(tokenSource.Token);          // Fire the search process and allow 10 seconds for receiver found events to call the handler.
            } catch (Exception ex) {
                Log.LogError("Exception on Loading: {ex} ", ex);
            }

            m_window = new MainWindow();
            this.m_window.Closed += M_window_Closed;
            m_window.Activate();
        }

        private void M_window_Closed(object sender, WindowEventArgs args) {
            //await MyHost.StopAsync();
            // Lets ged rid of our Singeltons in orderly manner...
            MyHost.Dispose();
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
