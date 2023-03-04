using RadioBrowser;
using RadioBrowser.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DLNAMediaRepos {

    public class DLNAAlbumRepository : IMediaRepository {
        Dictionary<string, (string albumName, List<(string url, string title)> tracks, string artist, string cdid)> CdAlbums = new();
        Dictionary<string, string> RadioStations = new();

        DLNAClient client = new();
        RadioBrowserClient radioBrowser = new();

        public DLNAAlbumRepository() {
            client.DLNADevices.CollectionChanged += DLNADevicesFound;

        }

        public async Task<int> LoadRadioStationsAsync() {
            var results = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions {
                Country = "Austria"
            });

            foreach (var st in results) {
                if (!RadioStations.ContainsKey(st.Url.ToString())) {
                    RadioStations.Add(st.Url.ToString(), st.Name);
                }
            }
            Console.WriteLine($"{RadioStations.Count} Stations found.");
            return results.Count;
        }


        public async Task<int> LoadAlbumsAsync() {
            int deviceCnt = await client.SearchingDevicesAsync();
            Console.WriteLine($"{CdAlbums.Count} Albums found in {deviceCnt} DLNA device(s).");

            return CdAlbums.Count;
        }

        private void DLNADevicesFound(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (DLNADevice device in e.NewItems) {
                    DLNADevice clonedDevice = new(device);
                    Console.WriteLine("Searching" + device.ModelName + " " + device.FriendlyName + " for album/track items...");
                    var rootContent = clonedDevice.GetDeviceContent("0");
                    rootContent.ForEach(item => {
                        ParseItems(clonedDevice, item);
                    });
                }
            }
        }

        private void ParseItems(DLNADevice device, DLNAObject item) {
            //Console.WriteLine(item.ClassName + " - " + item.Name);
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

                (string albumName, List<(string url, string title)> tracks, string artist, string cdid) album = (cd.Name, new List<(string url, string title)>(), art, "");
               
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
                album.cdid = (XX % 256).ToString("x2") + YYYY.ToString("x4") + tracks.Count.ToString("d2");
                CdAlbums.Add(cd.Name, album);
                //                Console.WriteLine(cdid);
            }
        }

        public List<(string, string)> GetCdTracks(int playIdx) {
            List<(string, string)> tracks = new ();
            var keys = CdAlbums.Keys.ToArray();
            if (keys.Length > 0) {
                playIdx %= keys.Length;
                var cd = CdAlbums[keys[playIdx]];
                tracks = cd.tracks;
                Console.WriteLine($"Retreived {tracks.Count} tracks fromo Album '{cd.albumName}'.");
            }
            return (tracks);
        }

        public (string url, string name) GetRadioStation(int playIdx) {

            (string url, string name) webradio = new("https://orf-live.ors-shoutcast.at/oe1-q2a", "st x");
            var keys = RadioStations.Keys.ToArray();
            if (keys.Length > 0) {
                playIdx %= keys.Length;
                webradio.url = keys[playIdx];
                webradio.name = RadioStations[keys[playIdx]];
            }
            return webradio;
        }

        public List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> GetAllAlbums() {
            return CdAlbums.Values.OrderBy(a=>a.artist).ToList();
        }

        public List<(string url, string name)> GetAllStations() {
            List< (string url, string name)> retVal = new List<(string url, string name)> ();
            var keys = RadioStations.Keys.ToArray();
            foreach ( var key in keys) {
                retVal.Add((key, RadioStations[key]));
            }
            return retVal;
        }

    }
}
