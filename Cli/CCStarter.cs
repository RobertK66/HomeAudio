using QueueCaster;
using QueueCaster.queue.models;
using Sharpcaster;
using Sharpcaster.Interfaces;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaStatus = QueueCaster.MediaStatus;

namespace Cli {
    public class CCStarter {
        private string ccName;
        private string appId;
        private QueueMediaChannel? mediaChannel;

        public CCStarter(string ccName, string appId) {
            this.ccName = ccName;
            this.appId = appId;
        }

        public async Task Connect() {
            IChromecastLocator locator = new MdnsChromecastLocator();
            var chromecasts = await locator.FindReceiversAsync();

            Console.WriteLine("CC Cnt:" + chromecasts.Count());
            var cc = chromecasts.Where(c => c.Name.StartsWith(ccName)).FirstOrDefault();
            if (cc != null) {
                Console.WriteLine("**** Status: " + cc.Status);

                var client = QueueCaster.ChromecastClient.CreateQueueCasterClient(null);
                var st = await client.ConnectChromecast(cc);
                st = await client.LaunchApplicationAsync(appId, true);

                mediaChannel = (QueueMediaChannel)client.GetChannel<IMediaChannel>();
            }
        }

        public async Task PlayLive(string url, string? name = null) {
            if (mediaChannel != null) {
                var media = new Media {
                    ContentUrl = url,
                    StreamType = StreamType.Live,
                    ContentType = "audio/mp4",
                    Metadata = new MediaMetadata() { Title = name ?? url }
                };
                await mediaChannel.LoadAsync(media);
                //await Play(media);
            }
        }

        public async Task PlayCdTracks(List<(string url, string name)> tracks) {
            if (mediaChannel != null) {
                QueueItem[]? qi = new QueueItem[tracks.Count];
                int i = 0;
                foreach (var (url, name) in tracks) {
                    var media = new Media {
                        ContentUrl = url,
                        StreamType = StreamType.Buffered,
                        ContentType = "audio/mp4",
                        Metadata = new MediaMetadata() { Title = name }
                    };
                    qi[i] = new QueueItem() { Media = media, OrderId = i, StartTime = 0 };
                    i++;
                }
                await mediaChannel.QueueLoadAsync(qi);
                //_ = await PlayQueue(qi);
            }
        }

        //public async Task Play(Media media) {
        //    if (mediaChannel != null) {
        //        _ = await mediaChannel.LoadAsync(media);
        //    }
        //}

        //public async Task<MediaStatus?> PlayQueue(QueueItem[] queue) {
        //    if (mediaChannel != null) {
        //        return await mediaChannel.QueueLoadAsync(queue);
        //    }
        //    return null;
        //}

        public async Task<MediaStatus?> PlayNext() {
            if (mediaChannel != null) {
                var st1 = await mediaChannel.GetStatusAsync();
                if (st1 != null) {
                    return await mediaChannel.QueueNextAsync(st1.MediaSessionId);
                }
            }
            return null;
        }

        public async Task<MediaStatus?> PlayPrev() {
            if (mediaChannel != null) {
                var st1 = await mediaChannel.GetStatusAsync();
                if (st1 != null) {
                    return await mediaChannel.QueuePrevAsync(st1.MediaSessionId);
                }
            }
            return null;
        }

    }
}
