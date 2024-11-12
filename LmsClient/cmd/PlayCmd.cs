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
    internal class PlayCmd : Command<PlayCmd.Settings> {
        private LmsClientRepos _lmsClient;
        public PlayCmd(LmsClientRepos lmsCr) {
            _lmsClient = lmsCr;
        }

        public sealed class Settings : CommandSettings {

            [Description("The ID of the item to be played.")]
            [CommandArgument(0, "[id]")]
            public string? Id { get; init; }

            [Description("The Lyrion Music Server to be used.")]
            [CommandOption("-s|--server")]
            [DefaultValue("http://192.168.177.65:9000/jsonrpc.js")]
            public String LmsBaseUrl { get; init; } = "";

            [Description("The ID of the LmsPlayer to be used.")]
            [CommandOption("-p|--playerid")]
            [DefaultValue("08:b6:1f:b7:ae:c8")]
            public String PlayerId { get; init; } = "";


            [Description("The Type of object to be played. One of LmsAlbum|LmsRadio.")]
            [CommandOption("-t|--type")]
            [DefaultValue(LmsType.LmsAlbum)]
            public LmsType ObjType { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
            _lmsClient.BaseUrl = settings.LmsBaseUrl;

            if (settings.ObjType == LmsType.LmsRadio) {
                _lmsClient.PlayRadio(settings.PlayerId, settings.Id);
            } else {
                int count = _lmsClient.PlayAlbum(settings.PlayerId, settings.Id).Result;
                AnsiConsole.MarkupLineInterpolated($"{count} Track(s) loaded.");
            }

            //foreach (LmsObject obj in objects) {
            //    AnsiConsole.MarkupLineInterpolated($"{obj.Id} = {obj.Name}");
            //}

            return 0;
        }
    }
}
