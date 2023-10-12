using AudioCollectionApi;
using Cli;
using DLNAMediaRepos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

public class Program : IHostedService {
    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();
                                });
        await host.RunConsoleAsync();
    }

    //private IAudioCollection Repos;
    private string appId = "";
    private string ccName= "";

    private string qcCommand;
    private int playIdx;
    private string outPath;


    public Program(IConfiguration conf) {
        Console.WriteLine("Program() Constructor called.");

        // Read apsettings.json confioguration
      //  Repos = new MediaRepositiory(conf.GetSection("WebRadio"), conf.GetSection("CD-Data"));

        appId = conf.GetValue<string>("CC-AppId", "CC1AD845") ?? "";
        ccName = conf.GetValue<string>("CC-Name") ?? "";

        // Read Command Line Parameters
        qcCommand = conf.GetValue<string>("qcCmd", "playRadio")??"";
        playIdx = conf.GetValue<int>("qcIdx", 0);
        outPath = conf.GetValue<string>("out", "./outdata.json")??"";

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

        // Start the Caster connect ...
        CCStarter ccs = new(ccName, appId);
        var waitForCaster = ccs.Connect();

        // Start the DLNA Search ...
        IMediaRepository DlnaRepos2 = new DLNAAlbumRepository(null);
        await DlnaRepos2.LoadAllAsync(true);
        //var waitForAlbums = DlnaRepos2.LoadAlbumsAsync();
        //var waitForRadioStations = DlnaRepos2.LoadRadioStationsAsync();


        if (qcCommand == "playCd") {
            // For playing CD we have to wait for both, the DLNA Repos and the connect beeing ready.
            //await Task.WhenAll(waitForCaster);//, waitForAlbums);
            var cat = DlnaRepos2.GetCdCategories().FirstOrDefault();
            if (cat != null) {
                var tracks = DlnaRepos2.GetCdRepository(cat.Id).ElementAtOrDefault(playIdx)?.Tracks;
                if (tracks != null) {
                    await ccs.PlayCdTracks(tracks);
                }
            }
        } else if (qcCommand == "playRadio") {
            //await Task.WhenAll(waitForCaster); //, waitForRadioStations);
            var cat = DlnaRepos2.GetRadioCategories().FirstOrDefault();
            if (cat != null) {
                var media = DlnaRepos2.GetRadioRepository(cat.Id).ElementAtOrDefault(playIdx);
                if (!String.IsNullOrEmpty(media?.ContentUrl)) {
                    await ccs.PlayLive(media.ContentUrl, media.Name);
                }
            }
        } else if (qcCommand == "next") {
            await waitForCaster;
            _ = await ccs.PlayNext();
        } else if (qcCommand == "prev") {
            await waitForCaster;
            _ = await ccs.PlayPrev();
        } else if (qcCommand == "writeCd") {
            //await Task.WhenAll(waitForAlbums);
            var cat = DlnaRepos2.GetCdCategories().FirstOrDefault();
            var aa = DlnaRepos2.GetCdRepository(cat.Id).ToList();
            // Convert tuples to JSON objects as anonym classes -> attributes have names and not 'ItemN' when JSON serialized.
            var track = new { ContentUrl = default(string), Name = default(string) };
            var tracks = Array.CreateInstance(track.GetType(), 1);
            var album = new { CDID = default(string), Name = default(string), Artist = default(string), Tracks = tracks, Picpath = default(string) };
            var CdArray = Array.CreateInstance(album.GetType(), aa.Count);
            int albumIdx = 0;
            foreach (var a in aa) {
                //Console.WriteLine(a.cdid + ":" + a.name + "[" + a.tracks.Count + "] / " + a.artist);
                tracks = Array.CreateInstance(track.GetType(), a.Tracks.Count);
                int idx = 0;
                foreach(var t in a.Tracks) {
                    tracks.SetValue(new { ContentUrl = t.ContentUrl, Name = t.Name }, idx++);
                }
                var alb = new { CDID = a.CDID, Name = a.Name, Artist = a.Artist, Tracks = tracks, Picpath=a.Picpath };
                CdArray.SetValue(alb, albumIdx++);
            }
            var CdRepos = new { CdRepos = CdArray };
            string albj = JsonConvert.SerializeObject(CdRepos, Formatting.Indented);
            File.WriteAllText(outPath, albj);
        } else if (qcCommand == "writeRadio") {
            //await Task.WhenAll(waitForRadioStations);
            var cat = DlnaRepos2.GetRadioCategories().FirstOrDefault();
            var st = DlnaRepos2.GetRadioRepository(cat.Id).ToList();
            var station = new { Name = default(string), ContentUrl = default(string)};
            var StationArray = Array.CreateInstance(station.GetType(), st.Count);
            int idx = 0;
            foreach (var s in st) {
                StationArray.SetValue(new { Name = s.Name, ContentUrl = s.ContentUrl}, idx++);
            }
            var WebRadio = new { WebRadio = StationArray };
            string radj = JsonConvert.SerializeObject(WebRadio, Formatting.Indented);
            File.WriteAllText(outPath, radj);
        }
        Console.WriteLine("Program.StartAsync() finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StopAsync() called.");
        return Task.CompletedTask;
    }


}