using DLNAMediaRepos;
using Microsoft.Extensions.Configuration;
using QueueCaster.queue.models;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli {
    public class MediaRepositiory :IMediaRepository  {
        List<QueueItem[]> CdQueues = new List<QueueItem[]>();
        List<Media> WebRadios = new List<Media>();

        public MediaRepositiory(IConfigurationSection webRadios, IConfigurationSection cds) {
            foreach (var item in webRadios.GetChildren()) {
                var media = new Media {
                    ContentUrl = item.GetValue<string>("ContentUrl"),
                    StreamType = StreamType.Live,
                    ContentType = "audio/mp4",
                    Metadata = new MediaMetadata() { Title = item.GetValue<string>("StationName") }
                };
                WebRadios.Add(media);
            }

            foreach (var item in cds.GetChildren()) {
                List<QueueItem> tracks = new List<QueueItem>();
                
                foreach(var t in item.GetSection("Tracks").GetChildren()) {
                    Media m = new Media {
                        ContentUrl = t.GetValue<string>("ContentUrl"),
                        StreamType = StreamType.Buffered,
                        ContentType = "audio/mp4",
                        Metadata = new MediaMetadata() { Title = t.GetValue<string>("Title") }
                    };
                    QueueItem qi = new QueueItem {
                        Media = m,
                    };
                    tracks.Add(qi);
                }
                CdQueues.Add(tracks.ToArray());
            }

        }

        public (string,string) GetRadioStation(int playIdx) {
            if (WebRadios.Count > 0) {
                playIdx = playIdx % WebRadios.Count;
                return (WebRadios[playIdx].ContentUrl, WebRadios[playIdx].Metadata.Title);
            } else {
                return ("", "");
            }
        }

        public List<(string, string)> GetCdTracks(int playIdx) {
            List<(string, string)> qi = new List<(string, string)>();
            if (CdQueues.Count > 0) {
                playIdx = playIdx % CdQueues.Count;
                var q = CdQueues[playIdx];
                foreach (var c in q) {
                    if (c?.Media != null) {
                        qi.Add((c.Media.ContentUrl,c.Media.Metadata.Title));
                    }
                }
            }
            return qi;
        }

        public List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> GetAllAlbums() {
            throw new NotImplementedException();
        }

        public List<(string url, string name)> GetAllStations() {
            throw new NotImplementedException();
        }
    }
}
