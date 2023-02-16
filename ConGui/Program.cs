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
using Sharpcaster.Models.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using System.Threading;
using System;

public class Program : IHostedService {

    private readonly ILogger<Program> _logger;
    private IConfiguration WebRadios;

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

        WebRadios = conf.GetSection("WebRadio");
    }
        

    //private void PrintConf(string pf, IConfigurationSection c) {
    //    Console.WriteLine(pf + c.Path + " " + c.Value);
    //    foreach (var kv in c.GetChildren()) {
    //        PrintConf(pf + "-", kv);
    //    }
    //}

    private System.Threading.Thread? tuiThread;
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
                ConsoleManager.AdjustBufferSize();  // Resize for Windows!
                ConsoleManager.ReadInput(input);
                Thread.Sleep(50);
            }
        } catch (ThreadInterruptedException) {
            _logger.LogDebug("TUI Thread canceled by InterruptException");
        }
    }



    
    private IControl CreateMainView() {
        var myTxtBlock = new TextBlock { Text = "Hello world" };
        myTextBox = new TextBox { Text = "Hello console" };


        var pan = new ConsoleGUI.Controls.Canvas();
        int l = 0;
        int t = 10;
        foreach (var item in WebRadios.GetChildren()) {
            _logger.LogDebug(item.GetValue<String>("ContentUrl"));
            var b = new Button { Content = new Button() { Content = new TextBlock() { Text = item.GetValue<String>("StationName") }, MouseOverColor = new Color(5,0,0) } };
            pan.Add(b, new Rect(l, t, 20, 1));
            l += 21;
            
        }

        //tabPanel = new TabPanel();
        tabPanel.AddTab("Radio Stations", pan);

        
    

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

