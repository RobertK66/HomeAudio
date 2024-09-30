using LmsRepositiory;
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

    [Description("Shows all available Objects of type(-t) wuth its IDs.")]
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
            [Description("The Lyrion Music Server to be used.")]
            public String LmsBaseUrl { get; init; } = "";

            [CommandOption("-t|--type")]
            [DefaultValue(LmsType.LmsAlbum)]
            [Description("One of LmsAlbum|LmsRadio|LmsPlayer.")]
            public LmsType ObjType { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
            _lmsClient.BaseUrl = settings.LmsBaseUrl;
            IEnumerable<LmsObject> objects;
            switch (settings.ObjType) {
                case LmsType.LmsRadio:
                    objects = _lmsClient.GetRadiosAsync().Result; 
                    break;
                case LmsType.LmsAlbum:
                    objects = _lmsClient.GetAlbumsAsync().Result.OrderBy(x => { int i=0; Int32.TryParse(x.Id, out i); return i; });
                    break;
                case LmsType.LmsPlayer:
                default:
                    objects = _lmsClient.GetPlayersAsync().Result;
                    break;
            }

            foreach (LmsObject obj in objects) {
                AnsiConsole.MarkupLineInterpolated($"{obj.Id} = {obj.Name}");
            }

            return 0;
        }
    }
}

// Play command for album:

// Request: "a5:41:d2:cd:cd:05 playlistcontrol cmd:load album_id:22<LF>"