using AudioCollectionApi;
using AudioCollectionApi.api;
using DLNAMediaRepos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using AudioCollectionApi.model;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cli {
    public class Program : IHostedService {
        public static async Task Main(string[] args) {
            IHostBuilder host = Host.CreateDefaultBuilder(args)
                                    .ConfigureLogging((c) => { c.SetMinimumLevel(LogLevel.Trace); })
                                    .ConfigureServices((hostContext, services) => {
                                        services.AddHostedService<Program>();
                                        services.AddSingleton<DLNAAlbumRepository>();
                                        services.AddSingleton<WebRadioRepository>();
                                    });
            await host.RunConsoleAsync();
        }

   
        private readonly string appId = "";
        private readonly string ccName = "";

        private readonly string qcCommand;
        private readonly int playIdx;
        private readonly string outPath;

        private readonly IMediaRepository albumRepository;
        private readonly IMediaRepository radioRepository;
        private readonly ILogger logger;    


        public Program(IConfiguration conf, DLNAAlbumRepository DlnaRepos, ILogger<Program> log, WebRadioRepository radioRepos) {
            albumRepository = DlnaRepos;
            radioRepository = radioRepos;
            logger = log;

            logger.LogDebug("Program() Constructor called.");
            
            appId = conf.GetValue("CC-AppId", "CC1AD845") ?? "";
            ccName = conf.GetValue<string>("CC-Name") ?? "";

            // Read Command Line Parameters
            qcCommand = conf.GetValue("qcCmd", "playRadio") ?? "";
            playIdx = conf.GetValue("qcIdx", 0);
            outPath = conf.GetValue("out", "./outdata.json") ?? "";

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
            logger.LogDebug("Program.StartAsync() called.");

            // Start the Caster connect ...
            CCStarter ccs = new(ccName, appId);
            var waitForCaster = ccs.Connect();

            // Start the DLNA Search ...
            //IMediaRepository DlnaRepos = new DLNAAlbumRepository();
            var waitForAlbums = Task.CompletedTask; //albumRepository.LoadAllAsync("dummy");

            // Start the Radio Search ...
            var waitForRadios = radioRepository.LoadAllAsync("dummy");

            if (qcCommand == "playCd") {
                // For playing CD we have to wait for both, the DLNA Repos and the connect beeing ready.
                await Task.WhenAll(waitForCaster, waitForAlbums);
                var cat = albumRepository.GetCategories().FirstOrDefault();
                if (cat != null) {
                    var tracks = albumRepository.GetMediaRepository(cat.Id).ElementAtOrDefault(playIdx)?.Content;
                    if (tracks != null) {
                        await ccs.PlayCdTracks(tracks.Cast<NamedUrl>().ToList());
                    }
                }
            } else if (qcCommand == "playRadio") {
                await Task.WhenAll(waitForRadios, waitForCaster);
                IEnumerable<MediaCategory> radioCategories  = radioRepository.GetCategories();
                var cat = radioCategories.ElementAtOrDefault(5);
                if ((cat != null)) {
                    var media = radioRepository.GetMediaRepository(cat.Id).FirstOrDefault();
                    if (!string.IsNullOrEmpty(media?.ContentUrl)) {
                        await ccs.PlayLive(media.ContentUrl, media.Name);
                    }
                }

            } else if (qcCommand == "next") {
                //await waitForCaster;
                _ = await ccs.PlayNext();
            } else if (qcCommand == "prev") {
                //await waitForCaster;
                _ = await ccs.PlayPrev();
            } else if (qcCommand == "writeCd") {
                await Task.WhenAll(waitForAlbums);
                var cat = albumRepository.GetCategories().FirstOrDefault();
                if (cat != null) {
                    var aa = albumRepository.GetMediaRepository(cat.Id).ToList();
                    // Convert tuples to JSON objects as anonym classes -> attributes have names and not 'ItemN' when JSON serialized.
                    var track = new { ContentUrl = default(string), Name = default(string) };
                    var tracks = Array.CreateInstance(track.GetType(), 1);
                    var album = new { type = "cd", CDID = default(string), Name = default(string), Artist = default(string), Tracks = tracks, Picpath = default(string) };
                    var CdArray = Array.CreateInstance(album.GetType(), aa.Count);
                    int albumIdx = 0;
                    foreach (var a in aa) {
                        if (a is Cd cd) {
                            //Console.WriteLine(a.cdid + ":" + a.name + "[" + a.tracks.Count + "] / " + a.artist);
                            tracks = Array.CreateInstance(track.GetType(), a.Content.Count);
                            int idx = 0;
                            foreach (var t in a.Content) {
                                tracks.SetValue(new { t.ContentUrl, t.Name }, idx++);
                            }
                            var alb = new { type = "cd", cd.CDID, a.Name, cd.Artist, Tracks = tracks, cd.Picpath };
                            CdArray.SetValue(alb, albumIdx++);
                        }
                    }
                    var CdRepos = new { CdRepos = CdArray };
                    string albj = JsonSerializer.Serialize(CdRepos);
                    File.WriteAllText(outPath, albj);

                    logger.LogInformation($"Cd file '{outPath}' written with {CdArray.Length} entries.");
                }
            } else if (qcCommand == "writeRadio") {
                //await Task.WhenAll(waitForRadioStations);
                //var cat = DlnaRepos2.GetRadioCategories().FirstOrDefault();
                //if (cat != null) {
                //    var st = DlnaRepos2.GetRadioRepository(cat.Id).ToList();
                //    var station = new { Name = default(string), ContentUrl = default(string) };
                //    var StationArray = Array.CreateInstance(station.GetType(), st.Count);
                //    int idx = 0;
                //    foreach (var s in st) {
                //        StationArray.SetValue(new { s.Name, s.ContentUrl }, idx++);
                //    }
                //    var WebRadio = new { WebRadio = StationArray };
                //    string radj = "";// JsonConvert.SerializeObject(WebRadio, Formatting.Indented);
                //    File.WriteAllText(outPath, radj);
                //}
            }
            logger.LogDebug("Program.StartAsync() finished.");
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            logger.LogDebug("Program.StopAsync() called.");
            return Task.CompletedTask;
        }


    }
}