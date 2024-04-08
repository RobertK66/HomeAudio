using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using AudioCollectionApi;
using AudioCollectionImpl;
using WinUiHomeAudio.model;
using Sharpcaster.Interfaces;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {

        public MainWindow m_window;

        public readonly IHost MyHost;
        private readonly ILogger<App> Log;


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();

            MyHost = Microsoft.Extensions.Hosting.Host.
                       CreateDefaultBuilder().
                       ConfigureServices((context, services) => {
                           //services.AddSingleton(logVm);
                           services.AddSingleton<IMediaRepository, JsonMediaRepository>();
                           ////services.AddSingleton<IMediaRepository, DLNAAlbumRepository>();
                           services.AddSingleton<AppSettings>();
                           services.AddSingleton<ChromeCastRepository>();
                           services.AddLogging(logging => {
                               logging.AddFilter(level => level >= LogLevel.Trace);
                               //    //.AddWinUiLogger((con) => {       // This adds our LogPanel as possible target (configure in appsettings.json)
                               //    //    con.LoggerVm = logVm;
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
            try {
                // start the search for CC Receivers in local network
                IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
                locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
                _ = locator.FindReceiversAsync(CancellationToken.None);          // Fire the search process and wait for receiver found events in the handler. No await here!
            } catch (Exception ex) {
                Log.LogError("Exception on Loading: {ex} ", ex);
            }

            m_window = new MainWindow();
            m_window.Activate();
        }

        private void Locator_ChromecastReceivedFound(object? sender, Sharpcaster.Models.ChromecastReceiver e) {
            MyHost.Services.GetRequiredService<ChromeCastRepository>().Add(e);
        }

    }
}
