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

    public class DLNAAlbumRepository  : IMediaRepository{
        Dictionary<String, ObservableCollection<Cd>> CdRepositories = new Dictionary<string, ObservableCollection<Cd>>();
        Dictionary<String, ObservableCollection<NamedUrl>> RadioRepositories = new Dictionary<string, ObservableCollection<NamedUrl>>();

        private ObservableCollection<MediaCategory> CdCategories = new ObservableCollection<MediaCategory>();
        private ObservableCollection<MediaCategory> RadioCategories = new ObservableCollection<MediaCategory>();



        Dictionary<string, (string albumName, List<(string url, string title)> tracks, string artist, string cdid, string picpath)> CdAlbums = new();
        Dictionary<string, string> RadioStations = new();

        DLNAClient client; // = new();
        RadioBrowserClient radioBrowser; //= new();

        private ILogger Log;
        private int IdCnt = 0;

        public DLNAAlbumRepository(ILogger<DLNAAlbumRepository> l) {
            Log = l;
            client = new DLNAClient();
            client.DLNADevices.CollectionChanged += DLNADevicesFound;
            radioBrowser = new RadioBrowserClient();
        }

        public async Task<int> LoadRadioStationsAsync() {
            Log.LogInformation("Starting advanced radio Search ...");
            var results = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions {
                Country = "Austria"
            });

            foreach (var st in results) {
                if ((st.Url != null) && (st.Name != null)) {

                    MediaCategory cat = new() { Id = "Radio-" + (IdCnt++), Name = string.IsNullOrEmpty(st.Tags[0]) ? "untagged": st.Tags[0] };
                    if (!RadioRepositories.ContainsKey(cat.Id)) {
                        RadioCategories.Add(cat);
                        RadioRepositories.Add(cat.Id, new ObservableCollection<NamedUrl>());
                    }
                    ObservableCollection<NamedUrl> rep = RadioRepositories[cat.Id];
                    if (rep.Where(r => r.Name == st.Name).Count() == 0) {
                        rep.Add(new NamedUrl() { Name = st.Name, ContentUrl = st.Url.ToString() });
                    }

                    if (!RadioStations.ContainsKey(st.Url.ToString())) {
                        RadioStations.Add(st.Url.ToString(), st.Url.ToString());
                    }
                }
            }
            Log.LogInformation("{count} Stations found.", RadioStations.Count);
            return results.Count;
        }


        public async Task<int> LoadAlbumsAsync() {
            Log.LogInformation("Starting DLNA Album search ...");
            int deviceCnt = await client.SearchingDevicesAsync();
            Log.LogInformation($"{CdAlbums.Count} Albums found in {deviceCnt} DLNA device(s).");

            return CdAlbums.Count;
        }

        private void DLNADevicesFound(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (DLNADevice device in e.NewItems) {
                    DLNADevice clonedDevice = new(device);

                    MediaCategory cat = new() { Id = "Cd-" + (IdCnt++), Name = device.FriendlyName };
                    if (!CdRepositories.ContainsKey(cat.Id)) {
                        CdCategories.Add(cat);
                        CdRepositories.Add(cat.Id, new ObservableCollection<Cd>());
                    }



                    Log.LogInformation("Searching" + device.ModelName + " " + device.FriendlyName + " for album/track items...");
                    var rootContent = clonedDevice.GetDeviceContent("0");
                    rootContent.ForEach(item => {
                        ParseItems(clonedDevice, item);
                    });
                }
            }
        }

        private void ParseItems(DLNADevice device, DLNAObject item) {
            string g;
            bool trackG = false;
            Log.LogInformation(item.ClassName + " - " + item.Name);
            if (item.ClassName.Equals("object.container")) {
                var children = device.GetDeviceContent(item.ID);
                foreach (var child in children) {
                    if (child.ClassName.Equals("object.container")) {
                        ParseItems(device, child);
                    } else if (child.ClassName.Equals("object.container.album.musicAlbum")) {
                        AddAlbum(device, child);
                    }
                }
            }

        }

        private void AddAlbum(DLNADevice device, DLNAObject cd) {
            if (!CdAlbums.ContainsKey(cd.Name)) {
                Console.Write(".");
                var cdXmlDocument = new XmlDocument();
                cdXmlDocument.LoadXml(cd.XMLDump);
                var artist = cdXmlDocument.GetElementsByTagName("upnp:artist").Item(0);
                string art = artist?.InnerText ?? string.Empty;
                var picpath = cdXmlDocument.GetElementsByTagName("upnp:albumArtURI").Item(0)?.FirstChild?.Value;


                (string albumName, List<(string url, string title)> tracks, string artist, string cdid, string picpath) album = (cd.Name, new List<(string url, string title)>(), art, "", picpath);
               
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
                        album.tracks.Add((r.InnerText, track.Name));
                    }
                    TimeSpan result;
                    if (TimeSpan.TryParse(ds, CultureInfo.InvariantCulture, out result)) {
                        trackStart += (int)result.TotalSeconds;             // Start second of next track
                        YYYY += (int)result.TotalSeconds;           
                    }
                });
                album.cdid = (XX % 256).ToString("x2") + YYYY.ToString("x4") + tracks.Count.ToString("x2");
                //Console.Write(album.cdid);
                //if (!CdAlbums.ContainsKey(album.cdid)) {
                    CdAlbums.Add(album.albumName, album);
                //Console.WriteLine();
                //} else {
                //    //Console.WriteLine(" !!!");
                //}

                var c = CdCategories.Where(c => c.Name.Equals(device.FriendlyName)).FirstOrDefault();
                ObservableCollection<Cd> rep = CdRepositories[c.Id];
                if (rep.Where(cd => cd.CDID == album.cdid).Count() == 0) {
                    var rcd = new Cd() { Name = album.albumName, CDID = album.cdid, Artist=album.artist, Picpath=album.picpath };
                    foreach (var t in album.tracks) {
                        rcd.Tracks.Add(new NamedUrl() { ContentUrl = t.url, Name = t.title });
                    }
                    rep.Add(rcd); 
                }

            }
        }

        public List<(string, string)> GetCdTracks(int playIdx) {
            List<(string, string)> tracks = new ();
            var keys = CdAlbums.Keys.ToArray();
            if (keys.Length > 0) {
                playIdx %= keys.Length;
                var cd = CdAlbums[keys[playIdx]];
                tracks = cd.tracks;
                Log.LogInformation($"Retreived {tracks.Count} tracks fromo Album '{cd.albumName}'.");
            }
            return (tracks);
        }

        public (string name, string url) GetRadioStation(int playIdx) {

            (string url, string name) webradio = new("https://orf-live.ors-shoutcast.at/oe1-q2a", "st x");
            var keys = RadioStations.Keys.ToArray();
            if (keys.Length > 0) {
                playIdx %= keys.Length;
                webradio.url = RadioStations[keys[playIdx]];
                webradio.name = keys[playIdx];
            }
            return webradio;
        }

        public List<(string name, List<(string url, string name)> tracks, string artist, string cdid, string picpath)> GetAllAlbums() {
            return CdAlbums.Values.OrderBy(a=>a.artist).ToList();
        }

        public List<(string name, string url)> GetAllStations() {
            List< (string url, string name)> retVal = new List<(string url, string name)> ();
            var keys = RadioStations.Keys.ToArray();
            foreach ( var key in keys) {
                retVal.Add((RadioStations[key], key));
            }
            return retVal;
        }

        public (string name, string url) GetRadioStation(string stationName) {
            (string url, string name) webradio = new();
            if (RadioStations.ContainsKey(stationName)) {
                var st = RadioStations[stationName];
                webradio.url = RadioStations[stationName];
                webradio.name = stationName;
            }
            return webradio;
        }

        public List<(string url, string name)> GetCdTracks(string cdid) {
            List<(string, string)> tracks = new();
            if (CdAlbums.ContainsKey(cdid)) {
                var cd = CdAlbums[cdid];
                tracks = cd.tracks;
            }
            return tracks;
        }

        public void LoadAll(object PersitenceContext) {
            var waitForAlbums = LoadAlbumsAsync();
            //var waitForRadioStations = LoadRadioStationsAsync();

            //if (PersitenceContext is bool waitForLoading) {
            //    //var t = Task.WhenAll(waitForRadioStations, waitForAlbums);
            //    waitForAlbums.Wait();
            //} 
        }

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

        public Task LoadAllAsync(object PersitenceContext) {
            throw new NotImplementedException();
        }
    }
}
