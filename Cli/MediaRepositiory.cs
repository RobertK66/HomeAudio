﻿using Microsoft.Extensions.Configuration;
using QueueCaster.queue.models;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli {
    public class MediaRepositiory {
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

        public Media? GetRadioStation(int playIdx) {
            if (WebRadios.Count > 0) {
                playIdx = playIdx % WebRadios.Count;
                return WebRadios[playIdx];
            } else {
                return null;
            }
        }

        public QueueItem[]? GetCdTracks(int playIdx) {
            QueueItem[]? qi = null;
            if (CdQueues.Count > 0) {
                playIdx = playIdx % CdQueues.Count;
                qi = CdQueues[playIdx];
            }
            return qi;
        }
    }
}