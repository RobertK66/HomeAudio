using AudioCollectionApi.api;
using HomeAudioViewModel;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices.JavaScript;
using System.Windows;

namespace WpfHomeAudio {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private readonly IHost MyHost;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static IServiceProvider ServiceProvider { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public App() {
            MyHost = Host.CreateDefaultBuilder().
                     //ConfigureHttpJsonOptions(options => {
                     //    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                     //}).
                     ConfigureServices((context, services) => {
                         //services.AddSingleton(logVm);
                         services.AddSingleton<IMediaRepository, LmsClientRepos>();
                         //services.AddSingleton<IMediaRepository, JsonMediaRepository>();
                         //services.AddSingleton<IMediaRepository, DLNAAlbumRepository>();
                         //services.AddSingleton<AppSettings>();
                         services.AddSingleton<IPlayerRepository, LmsClientRepos>();
                         //services.AddSingleton<IPlayerRepository, ChromeCastRepository>();
                         // TODO: read log levels from config ...
                         //services.AddLogging(logging => {
                         //    logging.AddFilter(level => level >= LogLevel.Trace)
                         //           .AddWinUiLogger((con) => {
                         //               // This adds our LogPanel as possible target (configure in appsettings.json)
                         //               con.LoggerVm = logVm;
                         //           });
                         //});

                         // Register all ViewModels.
                         services.AddSingleton<MainViewModel>();

                         // Register all the Windows of the applications.
                         services.AddTransient<MainWindow>();


                     }).
                     Build();

            ServiceProvider = MyHost.Services;
        }

        protected override async void OnStartup(StartupEventArgs e) {
            
            await MyHost.StartAsync();
            
            var window = ServiceProvider.GetRequiredService<MainWindow>();
            window.Show();

            base.OnStartup(e);
        }

    }

}
