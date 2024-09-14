using LmsClient.model;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LmsClient.cmd
{

    internal sealed class ListCmd : Command<ListCmd.Settings> {
        private LmsClientRepos _lmsClient;
        public ListCmd(LmsClientRepos lmsCr) {
            _lmsClient = lmsCr;
        }

        public sealed class Settings : CommandSettings {
            //[Description("Path to search. Defaults to current directory.")]
            //[CommandArgument(0, "[searchPath]")]
            //public string? SearchPath { get; init; }
            [CommandOption("-s|--server")]
            [DefaultValue("http://192.168.177.65:9000/")]
            public String LmsBaseUrl { get; init; } = "";

            [CommandOption("-t|--type")]
            [DefaultValue(LmsType.LmsAlbum)]
            public LmsType ObjType { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
            _lmsClient.BaseUrl = settings.LmsBaseUrl;
            IEnumerable<LmsObject> objects;
            switch (settings.ObjType) {
                case LmsType.LmsRadio:
                    objects = _lmsClient.GetRadios(); 
                    break;
                case LmsType.LmsAlbum:
                    objects = _lmsClient.GetAlbums().OrderBy(x => { int i=0; Int32.TryParse(x.Id, out i); return i; });
                    break;
                case LmsType.LmsPlayer:
                default:
                    objects = _lmsClient.GetPlayers();
                    break;
            }

            foreach (LmsObject obj in objects) {
                AnsiConsole.MarkupLineInterpolated($"{obj.Id} = {obj.Name}");
            }

            return 0;
        }
    }
}
