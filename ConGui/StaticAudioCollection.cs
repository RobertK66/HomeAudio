using AudioCollectionApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using AudioCollectionApi.api;
using AudioCollectionApi.model;

namespace ConGui {

    public class StaticAudioCollection : ITabedAudioCollection {

        public class AudioEntry : IAudioEntry {
            public String Name { get; set; } = "";
            public String? ContentUrl { get; set; }
            public List<AudioEntry> Tracks { get; set; } = [];

            // To cast we have to use LINQ and create a new typed List!
            //public List<IAudioEntry>? AudioTracks => Tracks.ToList<IAudioEntry>();

            // new syntax with collection expression: hmmmm... ;-)
            public List<IAudioEntry>? AudioTracks => [.. Tracks];
        }

        public class AudioTab(String name, int col, int row, int cellSize) : IAudioTab {
            private readonly List<AudioEntry> entries = [];

            public int Cols { get; set; } = col;
            public int Rows { get; set; } = row;
            public int CellSize { get; set; } = cellSize;
            public string TabName { get; set; } = name;

            public List<AudioEntry> GetAudioEntries() {
                return entries;
            }

            internal void AddAudioEntry(AudioEntry item) {
                entries.Add(item);
            }

            List<IAudioEntry> IAudioTab.GetAudioEntries() {
                return [.. entries];
            }
        }

        private readonly ILogger Log;

        readonly List<IAudioTab> MediaTabs = [];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0090:\"new(...)\" verwenden", Justification = "mag ich hier lieber lesbarere mit Klassennamen davor")]
        public StaticAudioCollection(IConfiguration conf, ILogger<StaticAudioCollection> logger, IMediaRepository mr) {
            Log = logger;

            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".\\";
            rootPath += "/repos";
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
                    MediaCategory? mc = mr.GetCategories().Where(c=>name.Equals(c.Name)).FirstOrDefault();
                    if (mc != null) {
                        foreach(var media in mr.GetMediaRepository(mc.Id)) {
                            AudioEntry entry = new() { Name = media.Name };
                            if (media.IsCollection) {
                                foreach (var tr in media.Content) {
                                    entry.Tracks.Add(new AudioEntry() { Name = tr.Name, ContentUrl = tr.ContentUrl });
                                }
                            } else {
                                entry.ContentUrl = media.ContentUrl;
                            }
                            at.AddAudioEntry(entry);
                        }
                    }
                    MediaTabs.Add(at);
                }
            }
            Log.LogDebug("{tabCount} Media Tabs created.", MediaTabs.Count);

        }

        public List<IAudioTab> GetAllTabs() {
            return MediaTabs;
        }

    }
}
