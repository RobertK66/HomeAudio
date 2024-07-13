using AudioCollectionApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public StaticAudioCollection(IConfiguration conf, ILogger<StaticAudioCollection> logger, IMediaRepository mr) {
            Log = logger;

            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".\\";
            mr.LoadAllAsync(rootPath).Wait();

            var TabedAudioConf = conf.GetSection("MediaTabs");
            foreach (var tab in TabedAudioConf.GetChildren()) {
                AudioTab at = new AudioTab(tab.GetValue<string>("TabName") ?? "<unknown>",
                                           tab.GetValue<int>("Cols"),
                                           tab.GetValue<int>("Rows"),
                                           tab.GetValue<int>("CellSize"));
                String? contentFilePath = tab.GetValue<string>("Content");
                if (!String.IsNullOrEmpty(contentFilePath)) {

                    string name = Path.GetFileNameWithoutExtension(contentFilePath);
                    MediaCategory? mc = mr.GetCdCategories().Where(c=>c.Name.Equals(name)).FirstOrDefault();
                    if (mc != null) {
                        foreach(var cd in mr.GetCdRepository(mc.Id)) {
                            AudioEntry entry = new AudioEntry();
                            entry.Name = cd.Name;
                            foreach(var tr in cd.Tracks) {
                                entry.Tracks.Add(new AudioEntry() { Name = tr.Name, ContentUrl=tr.ContentUrl });
                            }
                            at.AddAudioEntry(entry);
                        }
                    }
                    mc = mr.GetRadioCategories().Where(c => c.Name.Equals(name)).FirstOrDefault();
                    if (mc != null) {
                        foreach (var radio in mr.GetRadioRepository(mc.Id)) {
                            AudioEntry entry = new AudioEntry();
                            entry.Name = radio.Name;
                            entry.ContentUrl = radio.ContentUrl;
                            at.AddAudioEntry(entry);
                        }
                    }
                    //using (StreamReader file = File.OpenText(contentFilePath)) {
                    //    JsonSerializer serializer = new JsonSerializer();
                    //    AudioEntry[]? entries = (AudioEntry[]?)serializer.Deserialize(file, typeof(AudioEntry[]));
                    //    if (entries != null && entries.Length > 0) {
                    //        foreach (var item in entries) {
                    //            at.AddAudioEntry(item);
                    //        }
                    //    }
                    //}
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
