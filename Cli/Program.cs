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
    //private DLNAAlbumRepository DlnaRepos2;
    private string appId = "";
    private string ccName= "";

    private string qcCommand;
    private int playIdx;

    public Program(IConfiguration conf) {
        Console.WriteLine("Program() Constructor called.");

        // Read apsettings.json confioguration
        Repos = new MediaRepositiory(conf.GetSection("WebRadio"), conf.GetSection("CD-Data"));
        //DlnaRepos2 = new DLNAAlbumRepository();
        //DlnaRepos = new DLNAMediaRepository();
        //_ = DlnaRepos.SerarchDevices();

        appId = conf.GetValue<string>("CC-AppId", "CC1AD845");
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

        DLNAAlbumRepository DlnaRepos2 = new();
        var waitForAlbums = DlnaRepos2.LoadAlbumsAsync();

        CCStarter ccs = new(ccName, appId);
        var waitForCaster = ccs.Connect();

        //await Task.WhenAll(waitForAlbums, waitForCaster);

        if (qcCommand == "playRadio") {
            await waitForCaster;
            var media = Repos.GetRadioStation(playIdx);
            if (!String.IsNullOrEmpty(media.url)) {
                await ccs.PlayLive(media.url, media.name);
            }
        } else if (qcCommand == "playCd") {
            await Task.WhenAll(waitForCaster, waitForAlbums);
            Console.WriteLine($"{waitForAlbums.Result} Albums found.");
            var tracks = DlnaRepos2.GetCdTracks(playIdx);
            if (tracks != null) {
                await ccs.PlayCdTracks(tracks);
            }
        } else if (qcCommand == "next") {
            await waitForCaster;
            _ = await ccs.PlayNext();
        } else if (qcCommand == "prev") {
            await waitForCaster;
            _ = await ccs.PlayPrev();
        }
        Console.WriteLine("Program.StartAsync() finished.");
    }

  

    public Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StopAsync() called.");
        return Task.CompletedTask;
    }



}