using AudioCollection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static ConGui.StaticAudioCollection;

namespace ConGui {


    public class StaticAudioCollection : ITabedAudioCollection {

        public class AudioEntry : IAudioEntry {
            public String Name { get; set; } = "";
            public String? ContentUrl { get; set; }
            public List<AudioEntry> Tracks { get; set; } = new List<AudioEntry>();

            // To cast we have to use LINQ and create a new typed List!
            public List<IAudioEntry>? AudioTracks => Tracks.ToList<IAudioEntry>();
        }

        public class AudioTab : IAudioTab {
            private readonly List<AudioEntry> entries = new();

            public int Cols { get; set; }
            public int Rows { get ; set ; }
            public int CellSize { get; set; }
            public string TabName { get ; set ; }

            public AudioTab(String name, int col, int row, int cellSize) {
                TabName = name;
                Cols = col;
                Rows = row;
                CellSize = cellSize;
            }

            public List<AudioEntry> GetAudioEntries() {
                return entries;
            }

            internal void AddAudioEntry(AudioEntry item) {
                entries.Add(item);
            }

            List<IAudioEntry> IAudioTab.GetAudioEntries() {
                return entries.ToList<IAudioEntry>();
            }
        }

        private readonly ILogger Log;

        //List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> Albums = new();
        //List<(string name, string url)> WebRadios = new();

        readonly List<IAudioTab> MediaTabs = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0090:\"new(...)\" verwenden", Justification = "mag ich hier lieber lesbarere mit Klassennamen davor")]
        public StaticAudioCollection(IConfiguration conf, ILogger<StaticAudioCollection> logger) {
            Log = logger;
            //var AlbumsConf = conf.GetSection("CdRepos");
            //foreach (var album in AlbumsConf.GetChildren()) {
            //    List<(string url, string name)> tracks = new();
            //    IConfiguration? tr = album.GetSection("Tracks");
            //    if (tr != null) {
            //        foreach (var t in tr.GetChildren()) {
            //            tracks.Add(new(t.GetValue<string>("ContentUrl") ?? "<unknown>", t.GetValue<string>("Name") ?? "<unknown>"));
            //        }
            //    }
            //    Albums.Add(new(album.GetValue<string>("Name") ?? "<unknown>",
            //                    tracks,
            //                    album.GetValue<string>("Artist") ?? "<unknown>",
            //                    album.GetValue<string>("CDID") ?? "<unknown>"));
            //}

            //var WebRadiosConf = conf.GetSection("WebRadio");
            //foreach (var station in WebRadiosConf.GetChildren()) {
            //    WebRadios.Add(new(station.GetValue<string>("Name") ?? "<unknown>",
            //                      station.GetValue<string>("ContentUrl") ?? "<unknown>"));
            //}
            //Log.LogDebug("Collection with {albumCount} Albums and {stationCount} stations created", Albums.Count, WebRadios.Count);

            var TabedAudioConf = conf.GetSection("MediaTabs");
            foreach (var tab in TabedAudioConf.GetChildren()) {
                AudioTab at = new AudioTab(tab.GetValue<string>("TabName") ?? "<unknown>",
                                           tab.GetValue<int>("Cols"),
                                           tab.GetValue<int>("Rows"),
                                           tab.GetValue<int>("CellSize"));
                String? contentFilePath = tab.GetValue<string>("Content");
                if (!String.IsNullOrEmpty(contentFilePath)) {

                    using (StreamReader file = File.OpenText(contentFilePath)) {
                        JsonSerializer serializer = new JsonSerializer();
                        AudioEntry[]? entries = (AudioEntry[]?)serializer.Deserialize(file, typeof(AudioEntry[]));
                        if (entries != null && entries.Length > 0) {
                            foreach (var item in entries) {
                                at.AddAudioEntry(item);
                            }
                        }
                    }
                    MediaTabs.Add(at);
                }
            }
            Log.LogDebug("{tabCount} Media Tabs created.", MediaTabs.Count);

        }

        //public List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> GetAllAlbums() {
        //    return Albums;
        //}

        //public List<(string name, string url)> GetAllStations() {
        //    return WebRadios;
        //}

        //public List<(string url, string name)> GetCdTracks(int albumIdx) {
        //    return Albums[albumIdx%Albums.Count].tracks;
        //}

        //public List<(string url, string name)> GetCdTracks(string cdid) {
        //    return Albums.Where(a=>a.cdid == cdid).FirstOrDefault().tracks;
        //}

        //public (string name, string url) GetRadioStation(int stationIdx) {
        //    return WebRadios[stationIdx];
        //}

        //public (string name, string url) GetRadioStation(string stationName) {
        //    return WebRadios.Where(st=>st.name == stationName).FirstOrDefault();
        //}

        public List<IAudioTab> GetAllTabs() {
            return MediaTabs;
        }

    }
}
