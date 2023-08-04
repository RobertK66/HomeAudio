using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using WinGuiPackaged.logger;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinGuiPackaged {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {
        private Window m_window;

        private ILoggerFactory loggerFactory;
        private LoggerVm logVm = new LoggerVm();
        private ILogger<App> Log;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.UnhandledException += App_UnhandledException;
            this.InitializeComponent();

            logVm = new LoggerVm();
            logVm.dq = DispatcherQueue.GetForCurrentThread();
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    //.AddFilter("Microsoft", LogLevel.Warning)
                    //.AddFilter("System", LogLevel.Warning)
                    .AddFilter("Sharpcaster", LogLevel.Trace)
                    .AddFilter("QueueCaster", LogLevel.Trace)
                    .AddFilter("WinGuiPackaged", LogLevel.Trace)
                    .AddConsole()
                    .AddWinUiLogger((con) => {       // This adds our LogPanel as possible target (configure in appsettings.json)
                        con.LoggerVm = logVm;
                     })
                    .AddDebug();
            });

            Log = loggerFactory.CreateLogger<App>();
            Log.LogInformation("Application started.");
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) {
            e.Handled = true;
            Log.LogCritical("Unhadeled Exception from " + sender.GetType().Name + " " + e.Message);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {

            var mvm = new MainViewModel(loggerFactory, logVm);
            m_window = new MainWindow() { AnyViewModel = mvm };
            m_window.Activate();

            
        }

        
    }
}
