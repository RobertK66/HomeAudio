using AudioCollectionApi;
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
        Dictionary<String, ObservableCollection<Cd>> CdRepositories = new Dictionary<string, ObservableCollection<Cd>>();
        Dictionary<String, ObservableCollection<NamedUrl>> RadioRepositories = new Dictionary<string, ObservableCollection<NamedUrl>>();

        private ObservableCollection<MediaCategory> CdCategories = new ObservableCollection<MediaCategory>();
        private ObservableCollection<MediaCategory> RadioCategories = new ObservableCollection<MediaCategory>();

        DLNAClient client; // = new();
        RadioBrowserClient radioBrowser; //= new();

        private ILogger Log;
        private int IdCnt = 0;

        public DLNAAlbumRepository(ILogger<DLNAAlbumRepository> l) {
            Log = l;
        }

        public async Task<int> LoadRadioStationsAsync() {
            Log.LogInformation("Starting advanced radio Search ...");
            radioBrowser = new RadioBrowserClient();
            var results = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions {
                Country = "Austria"
            });

            foreach (var st in results) {
                if ((st.Url != null) && (st.Name != null)) {
                    MediaCategory cat = new() { Id = "Radio-" + (IdCnt++), Name = string.IsNullOrEmpty(st.Tags[0]) ? "untagged" : st.Tags[0] };
                    if (!RadioRepositories.ContainsKey(cat.Id)) {
                        RadioCategories.Add(cat);
                        RadioRepositories.Add(cat.Id, new ObservableCollection<NamedUrl>());
                    }
                    ObservableCollection<NamedUrl> rep = RadioRepositories[cat.Id];
                    if (rep.Where(r => r.Name == st.Name).Count() == 0) {
                        rep.Add(new NamedUrl() { Name = st.Name, ContentUrl = st.Url.ToString() });
                    }

                    Log.LogInformation("{count} Stations found.", rep.Count);
                }
            }

            return results.Count;
        }


        public async Task<int> LoadAlbumsAsync() {
            Log.LogInformation("Starting DLNA Album search ...");
            client = new DLNAClient();
            client.DLNADevices.CollectionChanged += DLNADevicesFound;
            int deviceCnt = await client.SearchingDevicesAsync();
            Log.LogInformation("{deviceCnt} DLNA device(s) found.", deviceCnt);
            return deviceCnt;
        }

        private void DLNADevicesFound(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (DLNADevice device in e.NewItems) {
                    DLNADevice clonedDevice = new(device);

                    MediaCategory cat = new() { Id = "Cd-" + (IdCnt++), Name = device.FriendlyName };
                    if (CdCategories.Where(c => c.Id == cat.Id).Count() == 0) {
                        CdCategories.Add(cat);
                        CdRepositories.Add(cat.Id, new ObservableCollection<Cd>());
                    }
                    Log.LogInformation("Searching" + device.ModelName + " " + device.FriendlyName + " for album/track items...");
                    //var rootContent = clonedDevice.GetDeviceContent("0");
                    var rootContent = clonedDevice.GetDeviceContent("21");
                    rootContent.ForEach(async item => {
                        await ParseItems(clonedDevice, item);
                    });
                }
            }
        }

        private async Task ParseItems(DLNADevice device, DLNAObject item) {
            string g;
            Log.LogTrace("ParseItem [{id}] " + item.ClassName + " - " + item.Name, item.ID);
            await Task.Delay(400);
            if (item.ClassName.Equals("object.container")) {
                var children = device.GetDeviceContent(item.ID);
                foreach (var child in children) {
                    if (child.ClassName.Equals("object.container")) {
                        await ParseItems(device, child);
                    } else if (child.ClassName.Equals("object.container.album.musicAlbum")) {
                        if (child.Name.StartsWith("= All Songs")) {
                            // '= All Songs =' is a 'virtual composed 'album' container -> do not use here !
                            continue;
                        }
                        await AddAlbum(device, child);
                    }
                }
            }

        }

        private async Task AddAlbum(DLNADevice device, DLNAObject cd) {
            var c = CdCategories.Where(c => c.Name.Equals(device.FriendlyName)).FirstOrDefault();
            ObservableCollection<Cd> rep = CdRepositories[c.Id];

            if (rep.Where(rcd => cd.Name == rcd.Name).Count() == 0) {
                Log.LogInformation("{album} reading", cd.Name);
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
                        album.Tracks.Add(new NamedUrl() { ContentUrl = r.InnerText, Name = track.Name });
                    }
                    TimeSpan result;
                    if (TimeSpan.TryParse(ds, CultureInfo.InvariantCulture, out result)) {
                        trackStart += (int)result.TotalSeconds;             // Start second of next track
                        YYYY += (int)result.TotalSeconds;
                    }
                });

                album.CDID = (XX % 256).ToString("x2") + YYYY.ToString("x4") + tracks.Count.ToString("x2");

                if (rep.Where(cd => cd.CDID == album.CDID).Count() == 0) {
                    Log.LogInformation("{album} added.", cd.Name);
                    rep.Add(album);
                }
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
            Log.LogInformation("DLNA wait for start ...");
            await Task.Delay(100);
            Log.LogInformation("DLNA starting ...");
            var t1 = LoadAlbumsAsync();
            //var t2 = LoadRadioStationsAsync();
            await Task.WhenAll(t1); //, t2);
            Log.LogInformation("DLNA ready!!!!");
        }
    }
}
