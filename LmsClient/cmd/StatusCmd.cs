using Spectre.Console.Cli;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LmsRepositiory;

namespace LmsClient.cmd {
    [Description("Issues an immediate PLAY command for the item specified with type(-t) and ID.")]
    internal class StatusCmd : Command<StatusCmd.Settings> {
        private LmsClientRepos _lmsClient;
        public StatusCmd(LmsClientRepos lmsCr) {
            _lmsClient = lmsCr;
        }

        public sealed class Settings : CommandSettings {

            [Description("The Lyrion Music Server to be used.")]
            [CommandOption("-s|--server")]
            [DefaultValue("http://192.168.177.65:9000/jsonrpc.js")]
            public String LmsBaseUrl { get; init; } = "";

            [Description("The ID of the LmsPlayer to be used.")]
            [CommandOption("-p|--playerid")]
            [DefaultValue("08:b6:1f:b7:ae:c8")]
            public String PlayerId { get; init; } = "";


        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
            _lmsClient.BaseUrl = settings.LmsBaseUrl;

            var res = _lmsClient.GetPlayerStatusAsync(settings.PlayerId).Result;
            if (res != null) {
                AnsiConsole.MarkupLineInterpolated($"{res}");
            }

                        
            //foreach (LmsObject obj in objects) {
            //    AnsiConsole.MarkupLineInterpolated($"{obj.Id} = {obj.Name}");
            //}

            return 0;
        }
    }
}
