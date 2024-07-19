using AudioCollectionApi;
using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Microsoft.Extensions.Logging;
using RadioBrowser;
using RadioBrowser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DLNAMediaRepos {

    public class DLNAAlbumRepository : IMediaRepository {
        Dictionary<String, ObservableCollection<Cd>> CdRepositories = new();
        Dictionary<String, ObservableCollection<NamedUrl>> RadioRepositories = new();

        private ObservableCollection<MediaCategory> CdCategories = new();
        private ObservableCollection<MediaCategory> RadioCategories = new();

        DLNAClient client; // = new();
        RadioBrowserClient radioBrowser; //= new();

        private readonly ILogger? Log;
        private int IdCnt = 0;

        public DLNAAlbumRepository(ILogger<DLNAAlbumRepository>? l) {
            Log = l;
        }

        public async Task<int> LoadRadioStationsAsync() {
            //Log.LogInformation("Starting advanced radio Search ...");
            radioBrowser = new RadioBrowserClient();
            var results = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions {
                Country = "Austria"
            });

            foreach (var st in results) {
                if ((st.Url != null) && (st.Name != null)) {
                    var tag = string.IsNullOrEmpty(st.Tags[0]) ? "untagged" : st.Tags[0];
                    MediaCategory? cat = RadioCategories.Where(rc => rc.Name == tag).FirstOrDefault();
                    if (cat == null) {
                        cat = new("Radio-" + (IdCnt++)) { Name = tag };
                        RadioCategories.Add(cat);
                        RadioRepositories.Add(cat.Id, new ObservableCollection<NamedUrl>());
                    } 
                    ObservableCollection<NamedUrl> rep = RadioRepositories[cat.Id];
                    if (!rep.Where(r => r.Name == st.Name).Any()) {
                        var entry = new NamedUrl(st.Name, st.Url.ToString());
                        //cat.Entries.Add(entry);
                        rep.Add(entry);
                    }

                   // Log.LogInformation("{count} Stations found.", rep.Count);
                }
            }

            return results.Count;
        }


        public async Task<int> LoadAlbumsAsync() {
            //Log.LogInformation("Starting DLNA Album search ...");
            client = new DLNAClient();
            client.DLNADevices.CollectionChanged += DLNADevicesFound; 
            int deviceCnt = await client.SearchingDevicesAsync();
            Log?.LogInformation("{deviceCnt} DLNA device(s) found.", deviceCnt);
            return deviceCnt;
        }

        private void DLNADevicesFound(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (DLNADevice device in e.NewItems) {
                    DLNADevice clonedDevice = new(device);

                    MediaCategory cat = new("Cd-" + (IdCnt++)) { Name = device.FriendlyName };
                    if (!CdCategories.Where(c => c.Name == cat.Name).Any()) {
                        CdCategories.Add(cat);
                        CdRepositories.Add(cat.Id, new ObservableCollection<Cd>());
                    }
                    //Log.LogInformation("Searching {mName} {fName} for album/track items... ",device.ModelName,device.FriendlyName);
                    //var rootContent = clonedDevice.GetDeviceContent("0");
                    var rootContent = clonedDevice.GetDeviceContent("21");
                    rootContent.ForEach(async item => {
                        await ParseItems(clonedDevice, item);//.ConfigureAwait(false);
                    });
                }
            }
        }

        private async Task ParseItems(DLNADevice device, DLNAObject item) {

            //Log.LogTrace("ParseItem [{id}] {name} {cname}", item.ID, item.Name, item.ClassName );
            //await Task.Delay(400);
            if (item.ClassName.Equals("object.container")) {
                var children = device.GetDeviceContent(item.ID);
                foreach (var child in children) {
                    if (child.ClassName.Equals("object.container")) {
                        await ParseItems(device, child);//.ConfigureAwait(false);
                    } else if (child.ClassName.Equals("object.container.album.musicAlbum")) {
                        if (child.Name.StartsWith("= All Songs")) {
                            // '= All Songs =' is a 'virtual composed 'album' container -> do not use here !
                            continue;
                        }
                        await AddAlbum(device, child);//.ConfigureAwait(false);
                    }
                }
            }

        }

        private async Task AddAlbum(DLNADevice device, DLNAObject cd) {
            var c = CdCategories.Where(c => c.Name.Equals(device.FriendlyName)).FirstOrDefault();
            ObservableCollection<Cd> rep = CdRepositories[c.Id];

            if (!rep.Where(rcd => cd.Name == rcd.Name).Any()) {
                Log?.LogInformation("+");
                //Log.LogInformation("{album} reading", cd.Name);
                await Task.Delay(100);
                var cdXmlDocument = new XmlDocument();
                cdXmlDocument.LoadXml(cd.XMLDump);
                var artist = cdXmlDocument.GetElementsByTagName("upnp:artist").Item(0);
                string art = artist?.InnerText ?? string.Empty;
                var picpath = cdXmlDocument.GetElementsByTagName("upnp:albumArtURI").Item(0)?.FirstChild?.Value;

                Cd album = new Cd() { Name = cd.Name, Artist = art, Picpath = picpath };

                int trackStart = 2;         // Starting second of track 1
                int XX = 0;                 // Sum of starting seconds
                int YYYY = 0;               // Totasl Sum of track length
                var tracks = device.GetDeviceContent(cd.ID);
                tracks.ForEach(track => {
                    var myXmlDocument = new XmlDocument();
                    myXmlDocument.LoadXml(track.XMLDump);
                    var r = myXmlDocument.GetElementsByTagName("res").Item(0);
                    var ds = r.Attributes["duration"].Value;
                    XX += trackStart;
                    if (r != null) {
                        album.Tracks.Add(new NamedUrl(name: track.Name??"cc",contentUrl: r.InnerText??"xx"));
                    }
                    if (TimeSpan.TryParse(ds, CultureInfo.InvariantCulture, out TimeSpan result)) {
                        trackStart += (int)result.TotalSeconds;             // Start second of next track
                        YYYY += (int)result.TotalSeconds;
                    }
                });

                album.CDID = (XX % 256).ToString("x2") + YYYY.ToString("x4") + tracks.Count.ToString("x2");

                if (!rep.Where(cd => cd.CDID == album.CDID).Any()) {
                    //Log.LogInformation("{album} added.", cd.Name);
                    rep.Add(album);
                }
            } else {
                Log?.LogInformation(".");
            }
        }



        //public void LoadAll(object PersitenceContext) {
        //    var waitForAlbums = LoadAlbumsAsync();
        //    //var waitForRadioStations = LoadRadioStationsAsync();

        //    //if (PersitenceContext is bool waitForLoading) {
        //    //    //var t = Task.WhenAll(waitForRadioStations, waitForAlbums);
        //    //    waitForAlbums.Wait();
        //    //} 
        //}

        public ObservableCollection<Cd> GetCdRepository(string collectionid) {
            if (CdRepositories.ContainsKey(collectionid)) {
                return CdRepositories[collectionid];
            } else {
                return new ObservableCollection<Cd>();
            }
        }

        public ObservableCollection<NamedUrl> GetRadioRepository(string collectionid) {
            if (RadioRepositories.ContainsKey(collectionid)) {
                return RadioRepositories[collectionid];
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

        public async Task LoadAllAsync(object PersitenceContext) {
            Log?.LogInformation("DLNA wait for start ...");
            await Task.Delay(100);
            //Log.LogInformation("DLNA starting ...");
            await LoadAlbumsAsync();//.ConfigureAwait(false);
            //var t1 = LoadAlbumsAsync();
            //var t2 = LoadRadioStationsAsync();
            //await Task.WhenAll(t1); //, t2);
            Log?.LogInformation("DLNA ready!!!!");
        }

        public ObservableCollection<MediaCategory> GetCategories() {
            throw new NotImplementedException();
        }

        public ObservableCollection<IMedia> GetMediaRepository(string categoryId) {
            throw new NotImplementedException();
        }
    }
}
