using AudioCollectionApi;
using AudioCollectionApi.api;
using AudioCollectionApi.model;
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
    public class JsonMediaRepository2 :IMediaRepository2 {

        //private enum MediaType { None, Cd, Radio };
        private readonly ILogger? Log;

        Dictionary<String, ObservableCollection<IMedia>> Repositories = new();
        private ObservableCollection<MediaCategory> Categories = new ();
        
        public JsonMediaRepository2(ILoggerFactory? lf = null) {
            Log = lf?.CreateLogger<JsonMediaRepository2>();
        }

        private bool loading = false;
        private String? reLoadPath = null;
        public async Task LoadAllAsync(object rootPath) {
            if (!loading) {
                loading = true;
                if (rootPath is string dirPath) {
                    int i = 0;
                    foreach (var f in Directory.GetFiles(dirPath, "*.json")) {
                        Log?.LogDebug("Scanning {path} for media content.", f);
                        if (reLoadPath != null) {
                            break;
                        }
                        //await Task.Delay(2000);
                        await AddRepos(System.IO.Path.GetFileNameWithoutExtension(f), f);
                    }
                }
                loading = false;
                if (reLoadPath != null) {
                    var p = reLoadPath;
                    reLoadPath = null;
                    await LoadAllAsync(p);
                }
            } else {
                reLoadPath = rootPath as string;
            }
        }

        private async Task AddRepos(string reposid, string path) {
            var rep = new ObservableCollection<IMedia>();
            if (Repositories.ContainsKey(reposid)) {
                rep = Repositories[reposid];
                rep.Clear();
            } else {
                Repositories.Add(reposid, rep);
            }
            MediaCategory? cat = Categories.Where(c => c.Id == reposid).FirstOrDefault();
            if (cat == null) {
                cat = new MediaCategory(reposid) { Name = System.IO.Path.GetFileNameWithoutExtension(path) };
                Categories.Add(cat);
            }
            
            if (File.Exists(path)) {
                try {
                    var options = new JsonSerializerOptions {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                    };

                    using Stream reader = new FileStream(path, FileMode.Open);
                    var cont = await JsonSerializer.DeserializeAsync<List<BaseMedia>>(reader, options);
                    if (cont != null) {
                        foreach (var item in cont) {
                            var media = item as IMedia;
                            if (media != null) {
                                //cat.Entries.Add(media);
                                rep.Add(media);
                            }
                        }
                    }
                    Log?.LogInformation("Added {count} entries from {name}[{id}]", cont?.Count, cat.Name, reposid);
                } catch (Exception ex) {
                    // Silently skip all non media json ...
                    Log?.LogTrace("Exception beim Laden eines Repositories: {repName}, {ex}", path, ex);
                }
            }

        }

        public ObservableCollection<IMedia> GetMediaRepository(string reposid) {
            if (Repositories.ContainsKey(reposid)) {
                return Repositories[reposid];
            } else {
                return new ObservableCollection<IMedia>();
            }
        }
        
        public ObservableCollection<MediaCategory> GetCategories() {
            return Categories;
        }

    }
}
