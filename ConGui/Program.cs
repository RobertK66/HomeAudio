using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConGui.Logger;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using ConGui.Controls;
using AudioCollection;

namespace ConGui {
    public class Program : IHostedService, IInputListener {

        private readonly ILogger<Program> Log;

        //private CCStarter myCC;
        private readonly IAudioCollection MyCollection;
        private readonly IChromeCastWrapper MyCCW;

        public static async Task Main(string[] args) {
            IHostBuilder host = Host.CreateDefaultBuilder(args)
                                    .ConfigureServices((hostContext, services) => {
                                        services.AddSingleton<IAudioCollection, StaticAudioCollection>();

                                        // In order to get a hosted service able to be injected in a constructor, we register both a singelton and a service!
                                        services.AddSingleton<IChromeCastWrapper, ChromeCastWrapper>();
                                        // As hosted service needs a implementing class we have to cast here to the real impl, but injection of singelton can use the Interface....
                                        services.AddHostedService(sp => (ChromeCastWrapper)sp.GetRequiredService<IChromeCastWrapper>());

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

        public Program(ILogger<Program> logger, IAudioCollection audioCollection, IChromeCastWrapper ccw) {
            Log = logger;
            Log.LogDebug("Program - Constructor called.");
            MyCollection = audioCollection;
            MyCCW = ccw;
            MyCCW.StatusChanged += MyCC_StatusChanged;
        }

        //private void PrintConf(string pf, IConfigurationSection c) {
        //    Console.WriteLine(pf + c.Path + " " + c.Value);
        //    foreach (var kv in c.GetChildren()) {
        //        PrintConf(pf + "-", kv);
        //    }
        //}

        private Thread? tuiThread;
        private IInputListener[]? input;
        private static readonly LogPanel myLogPanel = new();
        private readonly TabPanel tabPanel = new();
        private readonly TextBlock ccStatusText = new() { Text = "Unknown" };

        public void OnInput(InputEvent inputEvent) {
            if (inputEvent.Key.Key == ConsoleKey.Add) {
                MyCCW.VolumeUp();
            } else if (inputEvent.Key.Key == ConsoleKey.Subtract) {
                MyCCW.VolumeDown();
            } else if (inputEvent.Key.Key == ConsoleKey.End) {
                MyCCW.PlayNext();
            } else if (inputEvent.Key.Key == ConsoleKey.Home) {
                MyCCW.PlayPrev();
            } else if (inputEvent.Key.Key == ConsoleKey.Escape) {
                MyCCW.Shutdown();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            myLogPanel.Lock = this;
            Log.LogInformation("Program.StartAsync() called.");

            ConsoleManager.Setup();
            ConsoleManager.Resize(new Size(150, 40));
            Monitor.Enter(this);
            ConsoleManager.Content = CreateMainView();
            Monitor.Exit(this);


            input = new IInputListener[] {
                myLogPanel,
                //myTextBox,
                tabPanel,
                this
            };

            //MouseHandler.Initialize();

            tuiThread = new Thread(() => {
                Log.LogDebug("TUI Thread started");
                try {
                    while (true) {
                        Monitor.Enter(this);
                        ConsoleManager.AdjustBufferSize();  // Resize for Windows!
                        ConsoleManager.ReadInput(input);
                        Monitor.Exit(this);
                        Thread.Sleep(50);
                        //await Task.Delay(50);
                    }
                } catch (Exception ex) {
                    Log.LogDebug(ex, "TUI Thread terminated with Exception.");
                } finally {
                    if (Monitor.IsEntered(this)) {
                        Monitor.Exit(this);
                    }
                }
            }) {
                Name = "My-TUI"
            };
            tuiThread.Start();

            Log.LogInformation("Program.StartAsync() finished.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log.LogInformation("Program.StopAsync() called.");
            if (tuiThread?.IsAlive ?? false) {
                tuiThread?.Interrupt();
            }
            return Task.CompletedTask;
        }


        private IControl CreateMainView() {

            var radioGrid = new SelectableGrid(3, 4, 16);
            MyCollection.GetAllStations().ForEach(st => {
                radioGrid.AddTextCell(st.name, st, RadioStationClicked);
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

            var albumGrid = new SelectableGrid(4, 10, 16);

            MyCollection.GetAllAlbums().ForEach(al => {
                albumGrid.AddTextCell(al.name, al, AlbumClicked);
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
                Log.LogDebug("Play selected station: {name}", st?.name);
                MyCCW.PlayLive(st?.url ?? "",
                                  st?.name ?? "");
            }
        }

        void AlbumClicked(object albumContext) {
            (string name, List<(string url, string name)> tracks, string artist, string cdid)? album = albumContext as (string name, List<(string url, string name)> tracks, string artist, string cdid)?;
            if (album != null) {
                Log.LogDebug("Play selected album: {name}", album?.name);
                var tracks = album?.tracks;
                if (tracks != null) {
                    MyCCW.PlayCdTracks(tracks);
                }
            }
        }

        private void MyCC_StatusChanged(object? sender, EventArgs e) {
            //CCWStatusEventArgs? args = e as CCWStatusEventArgs;
            //if (args != null) {
            if (e is CCWStatusEventArgs args) {
                string statusText = "";
                if (args.AppCount > 0) {
                    statusText = $"{args.AppName} {args.AppStatus}";
                }
                statusText += $" - {args.MediaStatus} ";
                if (args.FirstTrack) { statusText += "|"; } else { statusText += "<"; }
                statusText += "-";
                if (args.LastTrack) { statusText += "|"; } else { statusText += ">"; }
                ccStatusText.Text = $" Vol:{string.Format("{0:0.000}", args.VolumeLevel)} - {statusText}";
            }
        }

    }
}