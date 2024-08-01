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


namespace AudioCollectionImpl {
    public class JsonMediaRepository(ILoggerFactory? loggerFactory = null) : IMediaRepository {

        private readonly ILogger? Log = loggerFactory?.CreateLogger<JsonMediaRepository>();

        private JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private readonly Dictionary<String, ObservableCollection<IMedia>> Repositories = [];
        private readonly ObservableCollection<MediaCategory> Categories = [];
        private bool loading = false;
        private String? reLoadPath = null;
        public async Task LoadAllAsync(object rootPath) {

            if (!loading) {
                loading = true;

                if (rootPath is string dirPath) {
                    Log?.LogInformation("*****  Repos starts scanning dir " + dirPath);
                    Repositories.Clear();
                    Categories.Clear();
                    try {
                        foreach (var f in Directory.GetFiles(dirPath, "*.json")) {
                            Log?.LogInformation("Scanning {path} for media content.", f);
                            if (reLoadPath != null) {
                                break;
                            }
                            //await Task.Delay(2000);
                            await AddRepos(System.IO.Path.GetFileNameWithoutExtension(f), f);
                        }
                    } catch (Exception ex) {
                        Log?.LogError("***** Error loading files. " + ex.Message);
                    }
                    Log?.LogInformation("*****  Repos stops scanning dir " + dirPath);
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

        private ObservableCollection<IMedia> CreateOrUseRepository(string category) {
            var rep = new ObservableCollection<IMedia>();
            if (Repositories.TryGetValue(category, out ObservableCollection<IMedia>? value)) {
                rep = value;
                rep.Clear();
            } else {
                Repositories.Add(category, rep);
            }
            MediaCategory? cat = Categories.Where(c => c.Id == category).FirstOrDefault();
            if (cat == null) {
                cat = new MediaCategory(category) { Name = category };
                Categories.Add(cat);
            }
            return rep;
        }

        private async Task AddRepos(string reposid, string path) {
            var rep = CreateOrUseRepository(reposid);
            //var rep = new ObservableCollection<IMedia>();
            //if (Repositories.TryGetValue(reposid, out ObservableCollection<IMedia>? value)) {
            //    rep = value;
            //    rep.Clear();
            //} else {
            //    Repositories.Add(reposid, rep);
            //}
            //MediaCategory? cat = Categories.Where(c => c.Id == reposid).FirstOrDefault();
            //if (cat == null) {
            //    cat = new MediaCategory(reposid) { Name = System.IO.Path.GetFileNameWithoutExtension(path) };
            //    Categories.Add(cat);
            //}

            if (File.Exists(path)) {
                try {
                    using Stream reader = new FileStream(path, FileMode.Open);
                    var cont = await JsonSerializer.DeserializeAsync<List<BaseMedia>>(reader, jsonOptions);
                    if (cont != null) {
                        foreach (var item in cont) {
                            if (item is IMedia media) {
                                rep.Add(media);
                            } else {
                                Log?.LogInformation($"Entry '{item.Name}' with type '{item.GetType().Name}' can not be added as IMedia element. Add \"type\":\"radio\" or \"type\":\"cd\" to your json objects.");
                            }
                        }
                    }
                    Log?.LogInformation("Added {count} entries from {name}[{id}]", rep.Count, reposid, reposid);
                } catch (Exception ex) {
                    // Skip all non media json ...
                    Log?.LogTrace("Exception beim Laden eines Repositories: {repName}, {ex}", path, ex);
                }
            }

        }

        public ObservableCollection<IMedia> GetMediaRepository(string reposid) {
            if (Repositories.TryGetValue(reposid, out ObservableCollection<IMedia>? value)) {
                return value;
            } else {
                return [];
            }
        }

        public ObservableCollection<MediaCategory> GetCategories() {
            return Categories;
        }

        public async Task LoadReposAsync(string category, Stream reader) {
            var rep = CreateOrUseRepository(category);
            try {
                var cont = await JsonSerializer.DeserializeAsync<List<BaseMedia>>(reader, jsonOptions);
                if (cont != null) {
                    foreach (var item in cont) {
                        if (item is IMedia media) {
                            rep.Add(media);
                        } else {
                            Log?.LogInformation($"Entry '{item.Name}' with type '{item.GetType().Name}' can not be added as IMedia element. Add \"type\":\"radio\" or \"type\":\"cd\" to your json objects.");
                        }
                    }
                }
                Log?.LogInformation("Added {count} entries from {name}[{id}]", rep.Count, category, category);
            } catch (Exception ex) {
                // Skip all non media json ...
                Log?.LogTrace("Exception beim Laden eines Repositories:  " + ex.Message);
            } finally {
                reader?.Dispose();
            }
        }
    }
}

