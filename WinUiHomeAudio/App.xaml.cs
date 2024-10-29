using AudioCollectionApi.api;
using AudioCollectionImpl;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Sharpcaster;
using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
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
                           //services.AddSingleton<IMediaRepository, LmsClientRepos>();
                           services.AddSingleton<IMediaRepository, JsonMediaRepository>();
                           //services.AddSingleton<IMediaRepository, DLNAAlbumRepository>();
                           services.AddSingleton<AppSettings>();
                           //services.AddSingleton<IPlayerRepository, LmsClientRepos>();
                           services.AddSingleton<IPlayerRepository, ChromeCastRepository>();
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

                // Lets start to find the available players ...
                var playerRepos = MyHost.Services.GetRequiredService<IPlayerRepository>();
                playerRepos.PlayerFound += Repos_PlayerFound;
                _ = playerRepos.LoadAllAsync();

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

        private void Repos_PlayerFound(object? sender, IPlayerProxy pp) {
            IPlayerRepository playerRepos = MyHost.Services.GetRequiredService<IPlayerRepository>();
            var appSettings = MyHost.Services.GetRequiredService<AppSettings>();
            var myContext = new MyUiContext() { dq = DispatcherQueue.GetForCurrentThread() };
            
            if (pp != null) {
                pp.SetContext(myContext);
                if (!String.IsNullOrEmpty(appSettings.AutoConnectName) && pp.Name.StartsWith(appSettings.AutoConnectName)) {
                    Log.LogInformation("Initiate AutoConnect for Receiver '{CcrName}'", pp.Name);
                    //DispatcherQueue.GetForCurrentThread();
                    _ = playerRepos.TryConnectAsync(pp);

                    if (m_window != null) {
                        m_window.MainPage.SelectedChromecast = pp as IPlayerProxy;
                    }
                }
            }
        }
    }
}
