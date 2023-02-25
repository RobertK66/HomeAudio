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
using System.Linq;
using ConsoleGUI.Common;
using System.Collections.Generic;

public class Program : IHostedService, IInputListener {

    private readonly ILogger<Program> _logger;
    private IConfiguration WebRadios;

    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    services.AddSingleton<CCStarter>();
                                    //AddHostedService<CCStarter>();
                                    services.AddHostedService<Program>();
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


    private CCStarter myCC;
    public Program(IConfiguration conf, ILogger<Program> logger, CCStarter cc) {
        _logger = logger;
        _logger.LogDebug("*********************** Program() Constructor called.");
        myCC = cc;
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

    private int selected = -1;
    private int count = 0;
    List<Background> stations = new List<Background>();
    

    public void OnInput(InputEvent inputEvent) {
        if (inputEvent.Key.Key == ConsoleKey.Enter) {
            _logger.LogDebug("Play selected: " + WebRadios.GetChildren().ToList()[selected].GetValue<String>("StationName"));
            _ = myCC.PlayLive(WebRadios.GetChildren().ToList()[selected].GetValue<String>("ContentUrl")??"",
                              WebRadios.GetChildren().ToList()[selected].GetValue<String>("StationName"));
        
        } else if (inputEvent.Key.Key == ConsoleKey.Add) {
            _ = myCC.VolumeUp();

        } else if (inputEvent.Key.Key == ConsoleKey.Subtract) {
            _ = myCC.VolumeDown();
        } else if (selected >= 0) {
            Background old = stations[selected];
            if ((inputEvent.Key.Key == ConsoleKey.LeftArrow)) {
                selected -= 1;
                if (selected < 0) { selected = count-1; }
            } else if ((inputEvent.Key.Key == ConsoleKey.RightArrow)) {
                selected += 1;
                if (selected > count-1) { selected = 0; }
            }

            Background newStat = stations[selected];
            old.Color = new Color(22, 22, 22);
            newStat.Color = new Color(100, 0, 0);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Program.StartAsync() called.");

        ConsoleManager.Setup();
        ConsoleManager.Resize(new Size(150, 40));
        ConsoleManager.Content = CreateMainView();


        input = new IInputListener[] {
                myLogPanel,
                //myTextBox,
                tabPanel,
                this
            };

        //MouseHandler.Initialize();

        tuiThread = new Thread(new ThreadStart(TuiThread));
		tuiThread.Start();

        // Start the Caster connect ...
        //CCStarter ccs = new("Büro", "9B5A75B4");
        //var waitForCaster = ccs.Connect();

        _logger.LogInformation("Program.StartAsync() finished.");
        //return Task.CompletedTask;
        return myCC.StartAsync(cancellationToken);
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
        
        count = WebRadios.GetChildren().Count();
        if (count > 0) {
            foreach (var item in WebRadios.GetChildren()) {
                _logger.LogDebug(item.GetValue<String>("ContentUrl"));
                Color backgrdcol = new Color(22, 22, 22);
                var b = new Background { Content = new TextBlock() { Text = item.GetValue<String>("StationName") }, Color = backgrdcol };
                stations.Add(b);
                pan.Add(b, new Rect(l, t, 20, 1));
                l += 21;
            }
            selected = 0;
            stations[0].Color = new Color(100, 0, 0); 
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

