using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DLNAMediaRepos {

    public class DLNAMediaRepository2 : IMediaRepository {
        Dictionary<string, (string albumName, List<(string url, string title)> tracks)> CdAlbums = new Dictionary<string, (string albumName, List<(string url, string title)> tracks)>();

        public DLNAMediaRepository2() {
            DLNAClient client = new DLNAClient();
            client.DLNADevices.CollectionChanged += DLNADevices_CollectionChanged;
            client.StartSearchingForDevices();
        }

        public List<(string,string)> GetCdTracks(int playIdx) {
            List<(string, string)> trackURIs = new List<(string, string)>();
            var keys = CdAlbums.Keys.ToArray();
            playIdx %= keys.Length;
            var cd = CdAlbums[keys[playIdx]];
            return (cd.tracks);
        }

        public (string, string) GetRadioStation(int playIdx) {
            return ("https://orf-live.ors-shoutcast.at/oe1-q2a","st x");
        }

        private void DLNADevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (DLNADevice device in e.NewItems) {
                    DLNADevice clonedDevice = new DLNADevice(device);
                    Console.WriteLine("Searching" + device.ModelName + " " + device.FriendlyName + " for album/track items...");
                    var rootContent = clonedDevice.GetDeviceContent("0");
                    rootContent.ForEach(item => {
                        ParseItems(clonedDevice, item);
                    });
                }
            }
        }

        private void ParseItems(DLNADevice device, DLNAObject item) {
            Console.WriteLine(item.ClassName + " - " + item.Name);
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
                Console.WriteLine("Adding Album " + cd.ID);
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
    }
}
