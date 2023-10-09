using AudioCollectionApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AudioCollectionImpl
{


    public class JsonMediaRepository :IMediaRepository {

        private enum MediaType { None, Cd, Radio };

        private ILogger Log;

        Dictionary<String, ObservableCollection<Cd>> CdRepositories = new Dictionary<string, ObservableCollection<Cd>>();
        Dictionary<String, ObservableCollection<NamedUrl>> RadioRepositories = new Dictionary<string, ObservableCollection<NamedUrl>>();

        private ObservableCollection<MediaCategory> CdCategories = new ObservableCollection<MediaCategory>();
        private ObservableCollection<MediaCategory> RadioCategories = new ObservableCollection<MediaCategory>();

        public JsonMediaRepository(ILogger<JsonMediaRepository> l) {
            Log = l;
        }


        private bool loading = false;
        private String? reloadPath = null;
        public async Task LoadAllAsync(object rootPath) {
            if (!loading) {
                loading = true;
                if (rootPath is string dirPath) {
                    int i = 0;
                    foreach (var f in Directory.GetFiles(dirPath, "*.json")) {
                        if (reloadPath != null) {
                            Log.LogInformation("Stop and reload...");
                            break;
                        }
                        Log.LogInformation("Next Path {path}", f);
                        await Task.Delay(2000);
                        switch (await CheckForMediaJson(f)) {
                            case MediaType.None:
                            default:
                                Log.LogInformation("{path} seems not to be a JSON object array. Skipping.", f);
                                continue; ;

                            case MediaType.Cd:
                                await AddCdRepos("CD-" + (i++).ToString(), f);
                                break;

                            case MediaType.Radio:
                                await AddRadioRepos("Radio-" + (i++).ToString(), f);
                                break;

                        }

                    }
                }
                loading = false;
                if (reloadPath != null) {
                    var p = reloadPath;
                    reloadPath = null;
                    await LoadAllAsync(p);
                }
            } else {
                // lets buffer changed config path if requested. Last one wins!
                reloadPath = rootPath as string;
            }
        }

        //public void LoadAll(object rootPath) {
        //    if (rootPath is string dirPath) {
        //        int i = 0;
        //        foreach (var f in Directory.GetFiles(dirPath, "*.json")) {
        //            if (CheckForMediaJson(f)!=true) {
        //                Log.LogInformation("{path} seems not to be a JSON object array. Skipping.", f);
        //                continue;
        //            }
        //            if (File.ReadAllText(f).Contains("\"Tracks\"")) {
        //                string reposid = "CD-" + i++;
        //                AddCdRepos(reposid, f);
        //            } else {
        //                string reposid = "Radio-" + i++;
        //                AddRadioRepos(reposid, f);
        //            }
        //        }
        //    }
        //}

        private async Task<MediaType> CheckForMediaJson(string f) {
            MediaType retVal = MediaType.None;

            using (StreamReader reader = File.OpenText(f)) {
                char[] buffer = new char[20];
                await reader.ReadBlockAsync(buffer, 0, 20);

                var firstchars = new string(buffer).Where(c => !char.IsWhiteSpace(c)).ToArray();
                if (firstchars.Length > 2) {
                    if (firstchars[0] == '[' && firstchars[1] == '{') {
                        if (new String(firstchars).Contains("CDID")) {
                            retVal = MediaType.Cd;
                        } else {
                            retVal = MediaType.Radio;
                        }
                    }
                }
                reader.Close();
            }
             
            return retVal;
        }

        private async Task AddCdRepos(string reposid, string path) {
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
                    using (Stream reader = new FileStream(path, FileMode.Open)) {
                        var cont = await JsonSerializer.DeserializeAsync<List<Cd>>(reader);
                        if (cont != null) {
                            foreach (var item in cont) {
                                rep.Add(item);
                            }
                        }
                        Log.LogDebug("Added {count} cds from {path}", cont?.Count, path);
                    }

                } catch (Exception ex) {
                    Log.LogError("Exception beim Laden eines Repositories: {repName}, {ex}", path, ex);
                }
            }
           
        }



        private async Task AddRadioRepos(string reposid, string path) {
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
                    var cont = await JsonSerializer.DeserializeAsync<List<NamedUrl>>(new FileStream(path, FileMode.Open));
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

        public ObservableCollection<MediaCategory> GetRadioCategories() {
            return RadioCategories;
        }

        public ObservableCollection<MediaCategory> GetCdCategories() {
            return CdCategories;
        }
    }
}
