using AudioCollection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui {
    public class StaticAudioCollection : IAudioCollection {
        private readonly ILogger Log;

        List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> Albums = new();
        List<(string name, string url)> WebRadios = new();

        public StaticAudioCollection(IConfiguration conf, ILogger<StaticAudioCollection> logger) {
            Log = logger;
            var AlbumsConf = conf.GetSection("CdRepos");
            foreach (var album in AlbumsConf.GetChildren()) {
                List<(string url, string name)> tracks = new ();
                IConfiguration? tr = album.GetSection("Tracks");
                if (tr != null) {
                    foreach (var t in tr.GetChildren()) {
                        tracks.Add(new(t.GetValue<string>("ContentUrl") ?? "<unknown>", t.GetValue<string>("Name") ?? "<unknown>"));
                    }
                }
                Albums.Add(new(album.GetValue<string>("Name") ?? "<unknown>",
                                tracks,
                                album.GetValue<string>("Artist") ?? "<unknown>",
                                album.GetValue<string>("CDID") ?? "<unknown>"));
            }

            var WebRadiosConf = conf.GetSection("WebRadio");
            foreach (var station in WebRadiosConf.GetChildren()) {
                WebRadios.Add(new(station.GetValue<string>("Name") ?? "<unknown>",
                                   station.GetValue<string>("ContentUrl") ?? "<unknown>"));
            }
            Log.LogDebug("Collection with {albumCount} Albums and {stationCount} stations created", Albums.Count, WebRadios.Count);
        }

        public List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> GetAllAlbums() {
            return Albums;
        }

        public List<(string name, string url)> GetAllStations() {
            return WebRadios;
        }

        public List<(string url, string name)> GetCdTracks(int albumIdx) {
            return Albums[albumIdx%Albums.Count].tracks;
        }

        public List<(string url, string name)> GetCdTracks(string cdid) {
            return Albums.Where(a=>a.cdid == cdid).FirstOrDefault().tracks;
        }

        public (string name, string url) GetRadioStation(int stationIdx) {
            return WebRadios[stationIdx];
        }

        public (string name, string url) GetRadioStation(string stationName) {
            return WebRadios.Where(st=>st.name == stationName).FirstOrDefault();
        }
    }
}
