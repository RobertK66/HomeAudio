using RadioBrowser;
using RadioBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DLNAMediaRepos {

    public class DLNAAlbumRepository : IMediaRepository {
        Dictionary<string, (string albumName, List<(string url, string title)> tracks)> CdAlbums = new();
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
                (string albumName, List<(string url, string title)> tracks) album = (cd.Name, new List<(string url, string title)>());
                CdAlbums.Add(cd.Name, album);
                var tracks = device.GetDeviceContent(cd.ID);
                tracks.ForEach(track => {
                    var myXmlDocument = new XmlDocument();
                    myXmlDocument.LoadXml(track.XMLDump);
                    var r = myXmlDocument.GetElementsByTagName("res").Item(0);
                    if (r != null) {
                        album.tracks.Add((r.InnerText, track.Name));
                    }
                });
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
    }
}
