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
using AudioCollection;
using Microsoft.VisualBasic;

public class Program : IHostedService, IInputListener {

    private readonly ILogger<Program> Log;

    //private CCStarter myCC;
    private IAudioCollection MyCollection;
    private IChromeCastWrapper MyCCW;

    //private IConfiguration WebRadios;
    //private IConfiguration Albums;

    public static async Task Main(string[] args) {
        IHostBuilder host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((hostContext, services) => {
                                    //services.AddSingleton<CCStarter>();
                                    services.AddSingleton<IAudioCollection, StaticAudioCollection>();

                                    // In order to get a hosted service able to be injected in a constructor, we register both a singelton and a service!
                                    services.AddSingleton<IChromeCastWrapper, ChromeCastWrapper>();
                                    // As hosted service needs a implementing class we have to cast here to the real impl, but injection of singelton can use the Interface....
                                    services.AddHostedService<ChromeCastWrapper>(sp=>(ChromeCastWrapper)sp.GetRequiredService<IChromeCastWrapper>());

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


    
    public Program(IConfiguration conf, ILogger<Program> logger, IAudioCollection audioCollection, IChromeCastWrapper ccw) {
        Log = logger;
        Log.LogDebug("Program - Constructor called.");
        //myCC = cc;
        MyCollection = audioCollection;
        MyCCW = ccw;
        MyCCW.StatusChanged += MyCC_StatusChanged;
    }

    private void MyCC_StatusChanged(object? sender, EventArgs e) {
        CCWStatusEventArgs? args = e as CCWStatusEventArgs;
        if (args != null) {
            String statusText = "";
            if (args?.Status?.Applications?.Count > 0) {
                statusText = $"{args.Status.Applications[0].DisplayName} {args.Status.Applications[0].StatusText}"; 
            }
            ccStatusText.Text = $" Vol:{String.Format("{0:0.000}", args?.Status.Volume.Level)} - {statusText}";
        }
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
    private TextBlock ccStatusText = new TextBlock() { Text = "Unknown" };

    public void OnInput(InputEvent inputEvent) {
        if (inputEvent.Key.Key == ConsoleKey.Add) {
            MyCCW.VolumeUp();
        } else if (inputEvent.Key.Key == ConsoleKey.Subtract) {
            MyCCW.VolumeDown();
        } else if (inputEvent.Key.Key == ConsoleKey.End) {
            MyCCW.PlayNext();
        } else if (inputEvent.Key.Key == ConsoleKey.Home) {
            MyCCW.PlayPrev();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        Log.LogInformation("Program.StartAsync() called.");

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

        Log.LogInformation("Program.StartAsync() finished.");
        return Task.CompletedTask;
        //return myCC.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        Log.LogInformation("Program.StopAsync() called.");
        if (tuiThread?.IsAlive??false) {
            tuiThread?.Interrupt();
        }
        return Task.CompletedTask;
    }

    private void TuiThread() {
        try {
            Log.LogDebug("TUI Thread started");
            while (true) {
                ConsoleManager.AdjustBufferSize();  // Resize for Windows!
                ConsoleManager.ReadInput(input);
                Thread.Sleep(50);
            }
        } catch (ThreadInterruptedException) {
            Log.LogDebug("TUI Thread canceled by InterruptException");
        }
    }

    private IControl CreateMainView() {

        var radioGrid = new SelectableGrid(3, 4, 16);
        MyCollection.GetAllStations().ForEach( st => {
            radioGrid.AddTextCell(st.name, st, this.RadioStationClicked);
        });

        var rad = new Box {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = new Border {
                BorderStyle = BorderStyle.Single,
                Content = radioGrid //lines
            }
        };

        tabPanel.AddTab("Radio Stations", rad, radioGrid);

        var albumGrid = new SelectableGrid(4, 10, 16 );

        MyCollection.GetAllAlbums().ForEach(al => {
            albumGrid.AddTextCell(al.name, al, this.AlbumClicked);
        });

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
            Placement = DockPanel.DockedControlPlacement.Top,
            DockedControl = tabPanel,
            FillingControl = new DockPanel {
                Placement = DockPanel.DockedControlPlacement.Top,
                DockedControl = new Border() { Content = ccStatusText, BorderStyle = BorderStyle.Single },
                FillingControl = myLogPanel
            }
        };

       return mainwin;

    }

    void RadioStationClicked(object station) {
        (string name, string url)? st = station as (string name, string url)?;
        if (st != null) {
            Log.LogDebug("Play selected station: " + st?.name);
            MyCCW.PlayLive(st?.url ?? "",
                              st?.name ?? "");
        }
    }

    void AlbumClicked(object albumContext) {
        (string name, List<(string url, string name)> tracks, string artist, string cdid)? album = albumContext as (string name, List<(string url, string name)> tracks, string artist, string cdid)?;
        if (album != null) {
            Log.LogDebug("Play selected album: " + album?.name);
            Log.LogDebug("Queueing " + album?.tracks.Count + " tracks.");
            if (album?.tracks != null) {
                MyCCW.PlayCdTracks(album?.tracks);
            }
            
        }
    }

}

