using Cli;
using DLNAMediaRepos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public class Program : IHostedService {
    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();
                                });
        await host.RunConsoleAsync();
    }

    private IMediaRepository Repos;
    //private DLNAMediaRepository DlnaRepos;
    private IMediaRepository DlnaRepos2;
    private string appId = "";
    private string ccName= "";

    private string qcCommand;
    private int playIdx;

    public Program(IConfiguration conf) {
        Console.WriteLine("Program() Constructor called.");

        // Read apsettings.json confioguration
        Repos = new MediaRepositiory(conf.GetSection("WebRadio"), conf.GetSection("CD-Data"));
        DlnaRepos2 = new DLNAMediaRepository2();
        //DlnaRepos = new DLNAMediaRepository();
        //_ = DlnaRepos.SerarchDevices();

        appId = conf.GetValue<string>("CC-AppId", "B3419EF5");
        ccName = conf.GetValue<string>("CC-Name");
        
        // Read Command Line Parameters
        qcCommand = conf.GetValue<string>("qcCmd", "playRadio");
        playIdx = conf.GetValue<int>("qcIdx", 0);

        //foreach (var item in conf.GetChildren()) {
        //    PrintConf("", item);
        //}
    }

    //private void PrintConf(string pf, IConfigurationSection c) {
    //    Console.WriteLine(pf + c.Path + " " + c.Value);
    //    foreach (var kv in c.GetChildren()) {
    //        PrintConf(pf + "-", kv);
    //    }
    //}

    //ServiceBrowser serviceBrowser;
    public async Task StartAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StartAsync() called.");

        // DLNAClient client = new DLNAClient();
        // client.DLNADevices.CollectionChanged += DLNADevices_CollectionChanged;
        // client.StartSearchingForDevices();

        //var devices = await new Ssdp().SearchUPnPDevicesAsync("MediaServer");
        

        CCStarter ccs = new CCStarter(ccName, appId);
        await ccs.connect();
        if (qcCommand == "playRadio") {
            var media = Repos.GetRadioStation(playIdx);
            if (!String.IsNullOrEmpty(media.url)) {
                await ccs.PlayLive(media.url, media.name);
            }
        } else if (qcCommand == "playCd") {
           var tracks = DlnaRepos2.GetCdTracks(playIdx);
            if (tracks != null) {
                await ccs.PlayCdTracks(tracks);
            }
        } else if (qcCommand == "next") {
           _ = await ccs.PlayNext();
        } else if (qcCommand == "prev") {
           _ = await ccs.PlayPrev();
        }
        Console.WriteLine("Program.StartAsync() finished.");
    }

  

    public Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StopAsync() called.");
        return Task.CompletedTask;
    }



}