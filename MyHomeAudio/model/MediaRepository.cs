﻿using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace MyHomeAudio.model {
    public class MediaRepository {
        private ILogger Log;

        Dictionary<String, ObservableCollection<Cd>> CdRepositories = new Dictionary<string, ObservableCollection<Cd>>();
        Dictionary<String, ObservableCollection<NamedUrl>> RadioRepositories = new Dictionary<string, ObservableCollection<NamedUrl>>();

        public MediaRepository(ILogger<MediaRepository> l) {
            Log = l;
        }

        internal void AddCdRepos(string reposid, string path) {
            var rep = new ObservableCollection<Cd>();
            if (CdRepositories.ContainsKey(reposid)) {
                rep = CdRepositories[reposid];
                rep.Clear();
            } else {
                CdRepositories.Add(reposid, rep);
            }

            if (File.Exists(path)) {
                try {
                    var cont = JsonSerializer.Deserialize<List<Cd>>(File.ReadAllText(path));
                    if (cont != null) {
                        foreach (var item in cont) {
                            rep.Add(item);
                        }
                    }
                    Log.LogDebug("Added {count} cds from {path}", cont?.Count, path);
                } catch (Exception ex) {
                    Log.LogError("Exception beim Laden eines Repositories: {repName}, {ex}", path, ex);
                }
            }
           
        }



        internal void AddRadioRepos(string reposid, string path) {
            var rep = new ObservableCollection<NamedUrl>();
            if (RadioRepositories.ContainsKey(reposid)) {
                rep = RadioRepositories[reposid];
                rep.Clear();
            } else {
                RadioRepositories.Add(reposid, rep);
            }

            if (File.Exists(path)) {
                try {
                    var cont = JsonSerializer.Deserialize<List<NamedUrl>>(File.ReadAllText(path));
                    if (cont != null) {
                        foreach (var item in cont) {
                            rep.Add(item);
                        }
                    }
                    Log.LogDebug("Added {count} webradios from {path}", cont?.Count, path);
                } catch (Exception ex) {
                    Log.LogError("Exception beim Laden eines Repositories: {repName}, {ex}", path, ex);
                }
            }

        }

        internal ObservableCollection<Cd> GetCdRepository(string reposid) {
            if (CdRepositories.ContainsKey(reposid)) {
                return CdRepositories[reposid];
            } else {
                return new ObservableCollection<Cd>();
            }
        }

        internal ObservableCollection<NamedUrl> GetRadioRepository(string reposid) {
            if (RadioRepositories.ContainsKey(reposid)) {
                return RadioRepositories[reposid];
            } else {
                return new ObservableCollection<NamedUrl>();
            }
        }
    }
}
