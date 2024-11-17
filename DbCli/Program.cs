using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebRadioImpl;


public class Program : IHostedService {
    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                //.ConfigureLogging((cl) => {
                                //    cl.ClearProviders();    // This avoids logging output to console.
                                //    cl.AddDebug();          // This gives Logging in the Debug Console of VS. (configure in appsettings.json)    
                                //})
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();

                                    //var connectionString = "Server=(localdb)\\mssqllocaldb;Database=EFGetStarted.ConsoleApp.NewDb;Trusted_Connection=True;";
                                    var connectionString = "Server=THINKP-15\\DEVSERVER;Database=HomeAudio;Trusted_Connection=True;TrustServerCertificate=true;";
                                    services.AddSqlServer<MyDataContext>(connectionString);
                                });
        await host.RunConsoleAsync();
    }


    private MyDataContext MyDc { get; set; }

    public Program(IConfiguration conf, MyDataContext dc) {
        Console.WriteLine("Program() Constructor called.");
        MyDc = dc;
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
        var rec = new WebRadio() { Name = DateTime.Now.ToString(), Description = "Dummy Startup record", StreamingUrl = "" };
        MyDc.WebRadios.Add(rec);
        await MyDc.SaveChangesAsync();


        Console.WriteLine("Program.StartAsync() finished." + rec.Id );
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Program.StopAsync() called.");
        return Task.CompletedTask;
    }


}