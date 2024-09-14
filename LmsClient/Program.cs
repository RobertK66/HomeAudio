using Injection.Infrastructure;
using LmsClient.cmd;
using LmsClient.model;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System;



namespace LmsClient {
    internal class Program {


        static void Main(string[] args) {

            var registrations = new ServiceCollection();
            registrations.AddSingleton<LmsClientRepos>();
            var registrar = new TypeRegistrar(registrations);

            var app = new CommandApp(registrar);
            app.Configure(config => {
                config.AddCommand<ListCmd>("list");
#if DEBUG
                config.PropagateExceptions();
                config.ValidateExamples();
#endif
            });
            app.Run(args);
        }

      
    }
}
