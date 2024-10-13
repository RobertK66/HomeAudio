using Injection.Infrastructure;
using LmsClient.cmd;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System;



namespace LmsClient {
    internal class Program {
        private CommandApp cmdApp;
        private List<string> lineBuffer = new List<string>();
        private int bufferIdx = 0;

        public Program(CommandApp app) {
            this.cmdApp = app;
        }

        static void Main(string[] args) {

            var registrations = new ServiceCollection();
            registrations.AddSingleton<LmsClientRepos>();
            var registrar = new TypeRegistrar(registrations);
            var app = new CommandApp(registrar);
            
            app.Configure(config => {
                config.AddCommand<ListCmd>("list");
                config.AddCommand<PlayCmd>("play");
                config.AddCommand<StatusCmd>("status");
#if DEBUG
                config.PropagateExceptions();
                config.ValidateExamples();
#endif
            });

            if (args.Length == 0) {
                Program p = new Program(app);
                p.RunShell();
            } else {
                // run the single command given by Execution args
                app.Run(args);
            }
        }

        private void RunShell() {
            // Enter the "shell" CL loop
            string prompt = "lms>";
            while (true) {
                var line = GetLine(prompt);         //AnsiConsole.Prompt(new TextPrompt<string>(prompt).AllowEmpty());
                if (!String.IsNullOrEmpty(line)) {
                    if (line.Trim().ToLower().StartsWith("exit")) {
                        break;
                    }
                    if ((new List<string>() { "-?", "?", "help", "-help", "--help" }).Contains(line.Trim().ToLower())) {
                        line = "-h";
                    }
                    try {
                        String[] largs = line.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
                        if (largs.Length == 2) {
                            if (largs[0].ToLower() == "help") {
                                largs[0] = largs[1];
                                largs[1] = "-h";
                            }
                        }
                        cmdApp.Run(largs);
                    } catch (Exception cpe) {
                        AnsiConsole.WriteException(cpe);
                    }
                }
            }
        }

        private string GetLine(string prompt) {
            AnsiConsole.Markup(prompt);
            string line = "";
            int idx = 0;
            bool loop = true;
            while (loop) {
                if (Console.KeyAvailable) {
                    var k = Console.ReadKey();
                    switch (k.Key) {
                        case ConsoleKey.Enter:
                            bufferIdx = lineBuffer.Count;
                            loop = false;
                            break;
                        case ConsoleKey.LeftArrow:
                            if (idx > 0) {
                                AnsiConsole.Cursor.MoveLeft();
                                idx--;
                            }
                            break;
                        case ConsoleKey.RightArrow:
                            if (idx < line.Length) {
                                AnsiConsole.Cursor.MoveRight();
                                idx++;
                            }
                            break;
                        case ConsoleKey.Backspace:
                            if (idx > 0) {
                                line = line.Remove(idx - 1, 1);
                                idx--;
                                AnsiConsole.Markup(line.Substring(idx) + " ");
                                AnsiConsole.Cursor.MoveLeft(line.Length - idx + 1);
                            } else {
                                AnsiConsole.Cursor.MoveRight();
                            }
                            break;
                        case ConsoleKey.Delete:
                            if ((idx >= 0) && (idx < line.Length)) {
                                line = line.Remove(idx, 1);
                                AnsiConsole.Markup(line.Substring(idx) + " ");
                                AnsiConsole.Cursor.MoveLeft(line.Length - idx + 1);
                            }
                            break;

                        case ConsoleKey.UpArrow:
                            if (bufferIdx > 0) {
                                AnsiConsole.Cursor.MoveLeft(idx);
                                AnsiConsole.Markup(new string(' ', idx));
                                AnsiConsole.Cursor.MoveLeft(idx);
                                bufferIdx--;
                                line = lineBuffer[bufferIdx];
                                AnsiConsole.Markup(line);
                                idx = line.Length;
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            if (bufferIdx < lineBuffer.Count - 1) {
                                AnsiConsole.Cursor.MoveLeft(idx);
                                AnsiConsole.Markup(new string(' ',idx));
                                AnsiConsole.Cursor.MoveLeft(idx);
                                bufferIdx++;
                                line = lineBuffer[bufferIdx];
                                AnsiConsole.Markup(line);
                                idx = line.Length;
                            }
                            break;
                        case ConsoleKey.Tab:
                            // Convert Tab to single space. (Spectre Console places cursor on next tab position n.i. how to read current pos so we rerender whole line...)
                            line += " ";
                            idx++;
                            AnsiConsole.Markup("\r" + prompt + line);    
                            break;
                        default:
                            // The char is echoed by console alrerady
                            line += k.KeyChar;
                            idx++;
                            break;
                    }
                }
            }
            AnsiConsole.MarkupLine("");
            if (!String.IsNullOrEmpty(line)) {
                lineBuffer.Add(line);
                bufferIdx++;
            }
            return line;
        }
    }
}
