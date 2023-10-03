using AudioCollectionApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AudioCollectionImpl
{
    public class JsonMediaRepository :IMediaRepository {
        private ILogger Log;

        Dictionary<String, ObservableCollection<Cd>> CdRepositories = new Dictionary<string, ObservableCollection<Cd>>();
        Dictionary<String, ObservableCollection<NamedUrl>> RadioRepositories = new Dictionary<string, ObservableCollection<NamedUrl>>();

        private List<MediaCategory> CdCategories = new List<MediaCategory>();
        private List<MediaCategory> RadioCategories = new List<MediaCategory>();

        public JsonMediaRepository(ILogger<JsonMediaRepository> l) {
            Log = l;
        }

        public void LoadAll(object rootPath) {
            if (rootPath is string dirPath) {
                int i = 0;
                foreach (var f in Directory.GetFiles(dirPath, "*.json")) {
                    if (CheckForMediaJson(f)!=true) {
                        Log.LogInformation("{path} seems not to be a JSON object array. Skipping.", f);
                        continue;
                    }
                    if (File.ReadAllText(f).Contains("\"Tracks\"")) {
                        string reposid = "CD-" + i++;
                        AddCdRepos(reposid, f);
                    } else {
                        string reposid = "Radio-" + i++;
                        AddRadioRepos(reposid, f);
                    }
                }
            }
        }

        private bool CheckForMediaJson(string f) {
            var fs = File.OpenText(f);
            char[] buffer = new char[20];
            fs.ReadBlock(buffer, 0, 20);
            var firstchars = new string(buffer).Where(c => !char.IsWhiteSpace(c)).ToArray();
            if (firstchars.Length > 2) {
                if (firstchars[0] != '[' || firstchars[1] != '{') {
                    return false;
                }
            } else {
                return false;
            }
            return true;
        }

        private void AddCdRepos(string reposid, string path) {
            var rep = new ObservableCollection<Cd>();
            if (CdRepositories.ContainsKey(reposid)) {
                rep = CdRepositories[reposid];
                rep.Clear();
            } else {
                CdRepositories.Add(reposid, rep);
            }
            MediaCategory? cat = CdCategories.Where(c => c.Id == reposid).FirstOrDefault();
            if (cat == null) {
                CdCategories.Add(new MediaCategory(){ Id = reposid, Name= System.IO.Path.GetFileNameWithoutExtension(path) });
            } else {
                cat.Name = System.IO.Path.GetFileNameWithoutExtension(path);
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



        private void AddRadioRepos(string reposid, string path) {
            var rep = new ObservableCollection<NamedUrl>();
            if (RadioRepositories.ContainsKey(reposid)) {
                rep = RadioRepositories[reposid];
                rep.Clear();
            } else {
                RadioRepositories.Add(reposid, rep);
            }
            MediaCategory? cat = RadioCategories.Where(c => c.Id == reposid).FirstOrDefault();
            if (cat == null) {
                RadioCategories.Add(new MediaCategory() { Id = reposid, Name = System.IO.Path.GetFileNameWithoutExtension(path) });
            } else {
                cat.Name = System.IO.Path.GetFileNameWithoutExtension(path);
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

        public ObservableCollection<Cd> GetCdRepository(string reposid) {
            if (CdRepositories.ContainsKey(reposid)) {
                return CdRepositories[reposid];
            } else {
                return new ObservableCollection<Cd>();
            }
        }

        public ObservableCollection<NamedUrl> GetRadioRepository(string reposid) {
            if (RadioRepositories.ContainsKey(reposid)) {
                return RadioRepositories[reposid];
            } else {
                return new ObservableCollection<NamedUrl>();
            }
        }

        public IEnumerable<MediaCategory> GetRadioCategories() {
            return RadioCategories;
        }

        public IEnumerable<MediaCategory> GetCdCategories() {
            return CdCategories;
        }
    }
}
