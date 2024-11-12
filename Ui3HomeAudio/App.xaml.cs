using AudioCollectionApi.api;
using HomeAudioViewModel;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUiHomeAudio;

namespace Ui3HomeAudio {

    public partial class App : Application {

        private Window m_window;

        private readonly IHost MyHost;
        public static IServiceProvider ServiceProvider { get; private set; }

        private readonly IObservableContext _uiContext;

        public App() {
            this.InitializeComponent();

            // In WinUi we need this to get model IPropertyChanged events working without knowledge of UI-Threading ... 
            _uiContext = new MyUiContext() { dq = DispatcherQueue.GetForCurrentThread() };

            MyHost = Host.CreateDefaultBuilder().
                  ConfigureServices((context, services) => {
                      services.AddSingleton<IObservableContext>(_uiContext);
                      services.AddSingleton<IMediaRepository, LmsClientRepos>();
                      services.AddSingleton<IPlayerRepository, LmsClientRepos>();

                      services.AddLogging(
                         logging => {
                             logging.AddFilter(level => level >= LogLevel.Trace);
                         });

                      // Register all ViewModels.
                      services.AddSingleton<MainViewModel>();
                      services.AddHostedService<MainViewModel>((p)=>p.GetRequiredService<MainViewModel>()); // The Service uses the singelton as its instance!

                      // Register all the Windows of the applications. ? Is as Transient better here !? -> not for a single mainwin....
                      services.AddSingleton<MainWindow>();
                      services.AddSingleton<MainPage>();

                  }).
                  Build();

            ServiceProvider = MyHost.Services;

        }

     
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {

            await MyHost.StartAsync();

            m_window = ServiceProvider.GetRequiredService<MainWindow>();
            m_window.Content = ServiceProvider.GetRequiredService<MainPage>();
            m_window.Closed += M_window_Closed; ;
            m_window.Activate();
        }

        private async void M_window_Closed(object sender, WindowEventArgs args) {
            await MyHost.StopAsync();
        }
    }
}
