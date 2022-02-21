using Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages;
using Sharpcaster.Models.Media;
using System.Reflection;

public class Program : IHostedService {
    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();
                                });
        await host.RunConsoleAsync();
    }

    private MediaRepositiory Repos;
    private string appId = "";
    private string ccName= "";

    private string qcCommand;
    private int playIdx;

    public Program(IConfiguration conf) {
        Console.WriteLine("Program() Constructor called.");

        // Read apsettings.json confioguration
        Repos = new MediaRepositiory(conf.GetSection("WebRadio"), conf.GetSection("CD-Data"));
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
   
    public async Task StartAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StartAsync() called.");
        CCStarter ccs = new CCStarter(ccName, appId);
        await ccs.connect();
        if (qcCommand == "playRadio") {
            var media = Repos.GetRadioStation(playIdx);
            if (media != null) {
                await ccs.Play(media);
            }
        } else if (qcCommand == "playCd") {
            QueueItem[]? qi = Repos.GetCdTracks(playIdx);
            if (qi != null) {
                _ = await ccs.PlayQueue(qi);
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