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
using System.Runtime.CompilerServices;
using ConGui.Controls;

public class Program : IHostedService, IInputListener {

    private readonly ILogger<Program> _logger;
    private IConfiguration WebRadios;
    private IConfiguration Albums;

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
        Albums = conf.GetSection("CdRepos");
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

    //private int selected = -1;
    //private int count = 0;
    //List<Background> stations = new List<Background>();
    

    public void OnInput(InputEvent inputEvent) {
        if (inputEvent.Key.Key == ConsoleKey.Enter) {
            //_logger.LogDebug("Play selected: " + WebRadios.GetChildren().ToList()[selected].GetValue<String>("Name"));
            //_ = myCC.PlayLive(WebRadios.GetChildren().ToList()[selected].GetValue<String>("ContentUrl") ?? "",
            //                  WebRadios.GetChildren().ToList()[selected].GetValue<String>("Name"));

        } else if (inputEvent.Key.Key == ConsoleKey.Add) {
            _ = myCC.VolumeUp();

        } else if (inputEvent.Key.Key == ConsoleKey.Subtract) {
            _ = myCC.VolumeDown();
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

        var radioGrid = new SelectableGrid(3, 4, 16);
        foreach (var stat in WebRadios.GetChildren()) {
            var b = new TextBlock() { Text = (stat.GetValue<String>("Name") ?? "").PadRight(15).Substring(0, 15) + " " };
            radioGrid.Add(b, stat, this.RadioStationClicked);
            //    (s) => {
            //    IConfigurationSection? st = s as IConfigurationSection;
            //    if (st != null) {
            //        _logger.LogDebug("Play selected station: " + st.GetValue<String>("Name"));
            //        _ = myCC.PlayLive(st.GetValue<String>("ContentUrl") ?? "",
            //                          st.GetValue<String>("Name"));
            //    }
            //});
        }

        var rad = new Box {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = new Border {
                BorderStyle = BorderStyle.Single,
                Content = radioGrid //lines
            }
        };

        tabPanel.AddTab("Radio Stations", rad, radioGrid);

        var albumGrid = new SelectableGrid(4, 10, 16);
        foreach (var album in Albums.GetChildren()) {
            var b = new TextBlock() { Text = (album.GetValue<String>("Name") ?? "").PadRight(15).Substring(0, 15) + " " };
            albumGrid.Add(b, album, this.AlbumClicked);
            //    (a) => {
            //    IConfigurationSection? album = a as IConfigurationSection;
            //    if (album != null) {
            //        _logger.LogDebug("Play selected album: " + album.GetValue<String>("Name"));
            //        IConfiguration? tr = album.GetSection("Tracks");
            //        if (tr != null) {
            //            List<(string url, string name)> tracks = new List<(string, string)>();
            //            foreach (var t in tr.GetChildren()) {
            //                tracks.Add(new(t.GetValue<string>("ContentUrl") ?? "url", t.GetValue<string>("Name") ?? "name"));
            //            }
            //            _ = myCC.PlayCdTracks(tracks);
            //        }
            //    }
            //});
        }

        var cd = new Box {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = new Border {
                BorderStyle = BorderStyle.Single,
                Content = albumGrid //lines
            }
        };

        tabPanel.AddTab("CdCollection", cd, albumGrid);
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

    void RadioStationClicked(object stationConfig) {
        IConfigurationSection? st = stationConfig as IConfigurationSection;
        if (st != null) {
            _logger.LogDebug("Play selected station: " + st.GetValue<String>("Name"));
            _ = myCC.PlayLive(st.GetValue<String>("ContentUrl") ?? "",
                              st.GetValue<String>("Name"));
        }
    }


    void AlbumClicked(object albumConfiguration) {
        IConfigurationSection? album = albumConfiguration as IConfigurationSection;
        if (album != null) {
            _logger.LogDebug("Play selected album: " + album.GetValue<String>("Name"));
            IConfiguration? tr = album.GetSection("Tracks");
            if (tr != null) {
                List<(string url, string name)> tracks = new List<(string, string)>();
                foreach (var t in tr.GetChildren()) {
                    tracks.Add(new(t.GetValue<string>("ContentUrl") ?? "url", t.GetValue<string>("Name") ?? "name"));
                }
                _ = myCC.PlayCdTracks(tracks);
            }
        }
    }

}

