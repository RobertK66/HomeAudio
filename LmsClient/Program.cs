using Injection.Infrastructure;
using LmsClient.cmd;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LmsClient;

internal class Program {
    private ICommandApp cmdApp;
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
            // Enter the Command shell.
            app.RunShell("lms>");
        } else {
            // run a single command given by Execution args
            app.Run(args);
        }
    }
}
