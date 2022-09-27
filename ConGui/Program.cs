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
using ConGui;
using ConsoleGUI.Api;

public class Program : IHostedService {

    private readonly ILogger<Program> _logger;

    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddHostedService<Program>();
                                    services.AddHostedService<CCStarter>();
                                })
                                .ConfigureLogging((cl) => {
                                    cl.ClearProviders();                // This avoids logging output to console.
                                    cl.AddConGuiLogger((con) => {       // This adds our LogPanel as possible target (configure in appsettings.json)
                                        con.LogPanel = myLogPanel;      
                                    });
                                    cl.AddDebug();                      // This gives Logging in the Debug Console of VS. (configure in appsettings.json)
                                });
        await host.RunConsoleAsync();
    }

    public Program(IConfiguration conf, ILogger<Program> logger) {
        _logger = logger;
        _logger.LogDebug("*********************** Program() Constructor called.");

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

    private Thread? tuiThread;
    private IInputListener[]? input;
    private static LogPanel myLogPanel = new();
    private TextBox myTextBox = new();
    private TabPanel tabPanel = new();

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Program.StartAsync() called.");

        ConsoleManager.Setup();
        ConsoleManager.Resize(new Size(150, 40));
        ConsoleManager.Content = CreateMainView();

        input = new IInputListener[] {
                myLogPanel,
                myTextBox,
                tabPanel
            };

        //MouseHandler.Initialize();

        tuiThread = new Thread(new ThreadStart(TuiThread));
		tuiThread.Start();

        // Start the Caster connect ...
        //CCStarter ccs = new("Büro", "9B5A75B4");
        //var waitForCaster = ccs.Connect();

        _logger.LogInformation("Program.StartAsync() finished.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Program.StopAsync() called.");
        if (tuiThread?.IsAlive??false) {
            tuiThread?.Interrupt();
        }
        return Task.CompletedTask;
    }


   


    private void TuiThread() {
        try {
            _logger.LogDebug("TUI Thread started");
            while (true) {
                ConsoleManager.ReadInput(input);
                Thread.Sleep(20);
            }
        } catch (ThreadInterruptedException) {
            _logger.LogDebug("TUI Thread canceled by InterruptException");
        }
    }



    
    private IControl CreateMainView() {
        var myTxtBlock = new TextBlock { Text = "Hello world" };
        myTextBox = new TextBox { Text = "Hello console" };
        myLogPanel.Add("First Entry from code");

        //tabPanel = new TabPanel();
        tabPanel.AddTab("Radio Stations", new Box {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = myTxtBlock
        });

        tabPanel.AddTab("My Cd Collection", new Box {
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

