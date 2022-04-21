using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConGui.Logger;

public class Program : IHostedService {

    private readonly ILogger<Program> _logger;

    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();
                                })
                                .ConfigureLogging((cl) => {
                                    cl.ClearProviders();    // This avoids logging output to console.
                                    cl.AddConGuiLogger((con) => {
                                        con.LogPanel = myLogPanel;
                                    });
                                    cl.AddDebug();          // This gives Logging in the Debug Console of VS. (configure in appsettings.json)
                                });
        await host.RunConsoleAsync();
    }

    public Program(IConfiguration conf, ILogger<Program> logger) {
        _logger = logger;
        _logger.LogInformation("Program() Constructor called.");

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

    private Thread tuiThread;
    private IInputListener[] input;
    private static LogPanel myLogPanel = new();
    private TextBox myTextBox = new();

    public async Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Program.StartAsync() called.");

        ConsoleManager.Setup();
        ConsoleManager.Resize(new Size(150, 40));
        ConsoleManager.Content = CreateMainView();

        input = new IInputListener[] {
                myLogPanel,
                myTextBox
            };


        tuiThread = new Thread(new ThreadStart(TuiThread));
		tuiThread.Start();

		_logger.LogInformation("Program.StartAsync() finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Program.StopAsync() called.");
        if (tuiThread.IsAlive) {
            tuiThread.Interrupt();
        }
        return Task.CompletedTask;
    }


   


    private void TuiThread() {
        int i = 0;
        try {
            _logger.LogDebug("TUI Thread started");
            while (true) {
                ConsoleManager.ReadInput(input);
                Thread.Sleep(20);
                if (i++ >= 100) {
                    i = 0;
                    myLogPanel.Add("Log it " + Random.Shared.NextDouble());
                }
            }
        } catch (ThreadInterruptedException ex) {
            _logger.LogDebug("TUI Thread canceled by InterruptException");
        }


    }



    
    private IControl CreateMainView() {
        var myTxtBlock = new TextBlock { Text = "Hello world" };
        myTextBox = new TextBox { Text = "Hello console" };
        myLogPanel.Add("FirstLog");

        var tabPanel = new TabPanel();
        tabPanel.AddTab("game", new Box {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = myTxtBlock
        });

        tabPanel.AddTab("leaderboard", new Box {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = new Background {
                Color = new Color(45, 74, 85),
                Content = new Border {
                    BorderStyle = BorderStyle.Single,
                    Content = myTextBox
                }
            }
        });
        tabPanel.SelectTab(1);

        var mainwin = new DockPanel {
            Placement = DockPanel.DockedControlPlacement.Bottom,
            DockedControl = new Boundary {
                MaxHeight = 15,
                MinHeight = 15,
                Content = myLogPanel
            },
            FillingControl = tabPanel
        };


       return mainwin;


    }


}

