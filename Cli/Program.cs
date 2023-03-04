using Cli;
using DLNAMediaRepos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

public class Program : IHostedService {
    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();
                                });
        await host.RunConsoleAsync();
    }

    private IMediaRepository Repos;
    private string appId = "";
    private string ccName= "";

    private string qcCommand;
    private int playIdx;

    public Program(IConfiguration conf) {
        Console.WriteLine("Program() Constructor called.");

        // Read apsettings.json confioguration
        Repos = new MediaRepositiory(conf.GetSection("WebRadio"), conf.GetSection("CD-Data"));

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

    public async Task StartAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StartAsync() called.");

        // Start the DLNA Search ...
        DLNAAlbumRepository DlnaRepos2 = new();
        var waitForAlbums = DlnaRepos2.LoadAlbumsAsync();

        var waitForRadioStations = DlnaRepos2.LoadRadioStationsAsync();

        // Start the Caster connect ...
        CCStarter ccs = new(ccName, appId);
        var waitForCaster = ccs.Connect();
        
        if (qcCommand == "playCd") {
            // For playing CD we have to wait for both, the DLNA Repos and the connect beeing ready.
            await Task.WhenAll(waitForCaster, waitForAlbums);
          

            var tracks = DlnaRepos2.GetCdTracks(playIdx);
            if (tracks != null) {
                await ccs.PlayCdTracks(tracks);
            }
        } else if (qcCommand == "playRadio") {
            await Task.WhenAll(waitForCaster, waitForRadioStations);
            var media = DlnaRepos2.GetRadioStation(playIdx);
            if (!String.IsNullOrEmpty(media.url)) {
                await ccs.PlayLive(media.url, media.name);
            }
        } else if (qcCommand == "next") {
            await waitForCaster;
            _ = await ccs.PlayNext();
        } else if (qcCommand == "prev") {
            await waitForCaster;
            _ = await ccs.PlayPrev();
        }


        await Task.WhenAll(waitForCaster, waitForRadioStations, waitForAlbums);
        var aa = DlnaRepos2.GetAllAlbums();
        foreach (var album in aa) {
            Console.WriteLine(album.cdid + ":"+ album.name + "[" +album.tracks.Count+"] / " + album.artist);
        }

        var options = new JsonSerializerOptions {
            IncludeFields = true,
        };
        string jsonString = JsonSerializer.Serialize(aa, options);

        var st = DlnaRepos2.GetAllStations();
        foreach (var station in st) {
            Console.WriteLine(station.name + " [" + station.url + "]");
        }



        Console.WriteLine("Program.StartAsync() finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StopAsync() called.");
        return Task.CompletedTask;
    }


}