using Microsoft.Extensions.DependencyInjection;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages;
using Sharpcaster.Models.Media;
using System.Reflection;

//internal sealed class ConsoleHostedService : IHostedService {
//    private readonly ILogger _logger;
//    private readonly IHostApplicationLifetime _appLifetime;

//    public ConsoleHostedService(
//        ILogger<ConsoleHostedService> logger,
//        IHostApplicationLifetime appLifetime) {
//        _logger = logger;
//        _appLifetime = appLifetime;
//    }

//    public Task StartAsync(CancellationToken cancellationToken) {
//        Console.WriteLine("Hello, World!");

//        _appLifetime.ApplicationStarted.Register(() => {
//            Task.Run(async () => {
//                try {
//                    await OldMain();
//                } catch (Exception ex) {
//                    _logger.LogError(ex, "Unhandled exception!");
//                } finally {
//                    // Stop the application once the work is done
//                    _appLifetime.StopApplication();
//                }
//            });
//        });
//        return Task.CompletedTask;
//    }

//    public Task StopAsync(CancellationToken cancellationToken) {
//        Console.WriteLine("ByeBye, World!");
//        return Task.CompletedTask;
//    }



//}


public class Program {
    public static async Task Main(string[] args) {
        //await CreateHostBuilder(args).RunConsoleAsync();
        await OldMain();
    }

    //public static IHostBuilder CreateHostBuilder(string[] args) =>
    //    Host.CreateDefaultBuilder(args)
    //        .ConfigureServices((hostContext, services) => {
    //            services.AddHostedService<ConsoleHostedService>();
    //        });

    private static async Task OldMain() {
        IChromecastLocator locator = new MdnsChromecastLocator();
        var chromecasts = await locator.FindReceiversAsync();

        //If that does not return devices on desktop then you can use this, Where 192.168.1.2 is your machines local ip
        ///var chromecasts = await locator.FindReceiversAsync("192.168.1.2");
        ///
        Console.WriteLine("CC Cnt:" + chromecasts.Count());
        var cc = chromecasts.Where(c => c.Name.StartsWith("Büro")).FirstOrDefault();
        if (cc != null) {
            //string myAppId = "B3419EF5";
            string myAppId = "9B5A75B4";
            Console.WriteLine("**** Status: " + cc.Status);


            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IChromecastChannel, ConnectionChannel>();
            serviceCollection.AddTransient<IChromecastChannel, HeartbeatChannel>();
            serviceCollection.AddTransient<IChromecastChannel, ReceiverChannel>();
            serviceCollection.AddTransient<IChromecastChannel, QueueMediaChannel>();
            var messageInterfaceType = typeof(IMessage);


            foreach (var type in (from t in typeof(IConnectionChannel).GetTypeInfo().Assembly.GetTypes()
                                  where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                  select t)) {
                if (!type.Name.Equals("MediaStatusMessage")) {  // Skip this Message. We replace it with our Version including queue info
                    serviceCollection.AddTransient(messageInterfaceType, type);
                    Console.WriteLine("***** " + type.Name);
                }
            }

            // Add our own Message Classes handling the queued item lists in MediaStatusMessage
            foreach (var type in (from t in typeof(QueueItem).GetTypeInfo().Assembly.GetTypes()
                                  where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                  select t)) {
                serviceCollection.AddTransient(messageInterfaceType, type);
                Console.WriteLine("+++++ " + type.Name);
            }

            var client = new ChromecastClient(serviceCollection);
            var st = await client.ConnectChromecast(cc);
            st = await client.LaunchApplicationAsync(myAppId, true);

            QueueMediaChannel mmc = (QueueMediaChannel)client.GetChannel<IMediaChannel>();

            ////Example Play WebRadio Live stream
            //var media = new Media {
            //    ContentUrl = "http://orf-live.ors-shoutcast.at/wie-q1a",
            //    StreamType = StreamType.Live,
            //    ContentType = "audio/mp4",
            //    Metadata = new MediaMetadata() { Title = "Ö2-Wien" }
            //};
            // _ = await mmc.LoadAsync(media);

            //// Example Play (local CD) as Queue
            var media1 = new Media {
                ContentUrl = "http://192.168.177.65:50002/m/MP3/7826.mp3",
                StreamType = StreamType.Buffered,
                ContentType = "audio/mp4",
                Metadata = new MediaMetadata() { Title = "Track1" }
            };
            var media2 = new Media {
                ContentUrl = "http://192.168.177.65:50002/m/MP3/7827.mp3",
                StreamType = StreamType.Buffered,
                ContentType = "audio/mp4",
                Metadata = new MediaMetadata() { Title = "Track2" }
            };
            var media3 = new Media {
                ContentUrl = "http://192.168.177.65:50002/m/MP3/7828.mp3",
                StreamType = StreamType.Buffered,
                ContentType = "audio/mp4",
                Metadata = new MediaMetadata() { Title = "Track3" }
            };
            var media4 = new Media {
                ContentUrl = "http://192.168.177.65:50002/m/MP3/7829.mp3",
                StreamType = StreamType.Buffered,
                ContentType = "audio/mp4",
                Metadata = new MediaMetadata() { Title = "Track4" }
            };
            QueueItem[] qi = {  new QueueItem() { Media = media1 },
                                new QueueItem() { Media = media2 },
                                new QueueItem() { Media = media3 },
                                new QueueItem() { Media = media4 }
            };
            var r = await mmc.QueueLoadAsync(qi);

            //Example Next while playing queue
            //var st1 = await mmc.GetStatusAsync();
            //if (st1 != null) {
            //    _ = await mmc.QueueNextAsync(st1.MediaSessionId);
            //}

        }

    }
}