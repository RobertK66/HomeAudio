using AudioCollectionApi.api;
using HomeAudioViewModel;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Windows;

namespace WpfHomeAudio {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public static IServiceProvider ServiceProvider { get; private set; } = null!; // " = null!; " avoids the CS8618 'make this nullable' - warning! 

        private readonly IHost MyHost;

        public App() {

            // No need for this in WPF if you use BindingOperations.EnableCollectionSynchronization on all observableCollections....
            // IObservableContext uiContext = new MyUiContext(Dispatcher);

            // I think this is equivalent code !?....
            //var MyAppBuilder = Host.CreateApplicationBuilder();
            //MyAppBuilder.Logging.AddFilter(level => level >= LogLevel.Trace);
            //MyAppBuilder.Services.AddSingleton<IMediaRepository, LmsClientRepos>();
            //MyAppBuilder.Services.AddSingleton<IPlayerRepository, LmsClientRepos>();
            //MyAppBuilder.Services.AddSingleton<MainViewModel>();
            //MyAppBuilder.Services.AddSingleton<MainWindow>();

            // ...???.... This is the 'old' callback host configuration style ... ???? .... 
            var MyHostBuilder = Host.CreateDefaultBuilder().
                                     ConfigureLogging((context, logBuilder) => {
                                         // Todo use std config here....
                                         logBuilder.AddFilter(level => level >= LogLevel.Trace);
                                     }).
                                     ConfigureServices((context, services) => {
                                         //services.AddSingleton(uiContext);
                                         services.AddSingleton<IMediaRepository, LmsClientRepos>();
                                         services.AddSingleton<IPlayerRepository, LmsClientRepos>();

                                         // Register all ViewModels.
                                         // To get a Hosted Service with call to Startup beeng a Injectable Singelton we have to register it as both
                                         services.AddSingleton<MainViewModel>();                                                    // This is a normal Singleton
                                         services.AddHostedService<MainViewModel>(p => p.GetRequiredService<MainViewModel>());      // The hosted Service is constructed by using the Singelton

                                         // Register all the Windows of the applications.
                                         services.AddSingleton<MainWindow>();
                                     });

            //MyHost = MyAppBuilder.Build();
            MyHost = MyHostBuilder.Build();

            // This makes sure that we have a not nullable static Service Provider after first (and only) instance of App()
            ServiceProvider = MyHost.Services;  
        }

        protected override async void OnStartup(StartupEventArgs e) {
            
            await MyHost.StartAsync();
            
            var window = ServiceProvider.GetRequiredService<MainWindow>();
            window.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e) {
            await MyHost.StopAsync();
            base.OnExit(e);
        }

    }

}
