using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Microsoft.Extensions.Logging;
using RadioBrowser;
using RadioBrowser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace DLNAMediaRepos {
    public class WebRadioRepository : IMediaRepository {

        private ILogger Log;

        private Dictionary<String, ObservableCollection<IMedia>> RadioRepositories = new();
        private ObservableCollection<MediaCategory> RadioCategories = new();
        private RadioBrowserClient? radioBrowser; //= new();


        public WebRadioRepository(ILogger<WebRadioRepository> logger) {
            Log = logger;
        }

        public ObservableCollection<MediaCategory> GetCategories() {
            return RadioCategories;
        }

        public ObservableCollection<IMedia> GetMediaRepository(string categoryId) {
            if (RadioRepositories.ContainsKey(categoryId)) {
                return RadioRepositories[categoryId];
            } else {
                return new ObservableCollection<IMedia>();
            }
        }

        public async Task LoadAllAsync(object context) {
            int IdCnt = 0;
            radioBrowser = new RadioBrowserClient();

            AdvancedSearchOptions? options = context as AdvancedSearchOptions;
            if (options == null) {
                options = new AdvancedSearchOptions { Country = "Austria" };
            }

            var results = await radioBrowser.Search.AdvancedAsync(options);

            int cnt = 0;
            foreach (var st in results) {
                if ((st.Url != null) && (st.Name != null)) {
                    var tag = string.IsNullOrEmpty(st.Tags[0]) ? "untagged" : st.Tags[0];
                    MediaCategory? cat = RadioCategories.Where(rc => rc.Name == tag).FirstOrDefault();
                    if (cat == null) {
                        cat = new("R-" + (IdCnt++)) { Name = tag };
                        RadioCategories.Add(cat);
                        RadioRepositories.Add(cat.Id, new ObservableCollection<IMedia>());
                        Log.LogDebug($"Category '{cat.Name}' added.");
                    }
                    ObservableCollection<IMedia> rep = RadioRepositories[cat.Id];
                    if (!rep.Where(r => r.Name == st.Name).Any()) {
                        var entry = new NamedUrl(st.Name, st.Url.ToString());
                        //cat.Entries.Add(entry);
                        rep.Add(entry);
                        cnt++;
                        Log.LogDebug($"Added '{entry.Name}'");
                    }
                }
            }

            Log.LogInformation($"{cnt} Stations found in {RadioCategories.Count} categories.");
        }
    }
}
