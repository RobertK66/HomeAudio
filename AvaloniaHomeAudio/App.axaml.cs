using AudioCollectionApi.api;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using AvaloniaHomeAudio.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LmsRepositiory;
using HomeAudioViewModel;

namespace AvaloniaHomeAudio;



public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        // Register all the services needed for the application to run
        var collection = new ServiceCollection();

        collection.AddSingleton<MainViewModel>();                                                 
        //collection.AddHostedService<MainViewModel>(p => p.GetRequiredService<MainViewModel>());  // The hosted Service is constructed by using the Singelton

        collection.AddSingleton<IMediaRepository, LmsClientRepos>();
        collection.AddSingleton<IPlayerRepository, LmsClientRepos>();

        collection.AddLogging(logging => {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddDebug();
        });

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();

        var vm = services.GetRequiredService<MainViewModel>();
        _ = vm.StartAsync(new System.Threading.CancellationToken());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
