using QueueCaster.queue.messages;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Media;
using Sharpcaster.Messages.Receiver;
using Sharpcaster.Models.ChromecastStatus;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster {
    public  class QueueMediaChannel : MediaChannel {

        // overwrite the base method for Load media beacuse we have another MediaStatus class registered as is used there!
        // -> cast exception!
        public new async Task<MediaStatus?> LoadAsync(Media media, bool autoPlay = true) {
            var status = Client.GetChromecastStatus();
            var r =  await SendAsync<MediaStatusMessage>(new LoadMessage() { SessionId = status.Applications[0].SessionId, Media = media, AutoPlay = autoPlay }, status.Applications[0].TransportId);
            return r?.Status?.FirstOrDefault();
        }

        // Media Channel methods to handle Queue
        public async Task<MediaStatus?> GetStatusAsync() {
            var st = this.Client.GetChromecastStatus();
            var r = await SendAsync<MediaStatusMessage>(new GetStatusMessage(), st.Applications[0].TransportId);
            return r?.Status?.FirstOrDefault();
        }

        public async Task<MediaStatus?> QueueLoadAsync(QueueItem[] items) {
            var app = Client.GetChromecastStatus().Applications[0];
            var r = await SendAsync<MediaStatusMessage>(new QueueLoadMessage() { SessionId = app.SessionId, Items = items }, app.TransportId);
            return r?.Status?.FirstOrDefault();
        }

        public async Task<MediaStatus?> QueueNextAsync(long mediaSessionId) {
            var app = Client.GetChromecastStatus().Applications[0];
            var r = await SendAsync<MediaStatusMessage>(new QueueNextMessage() { MediaSessionId = mediaSessionId }, app.TransportId); 
            return r?.Status?.FirstOrDefault();
        }

        public async Task<MediaStatus?> QueuePrevAsync(long mediaSessionId) {
            var app = Client.GetChromecastStatus().Applications[0];
            var r = await SendAsync<MediaStatusMessage>(new QueuePrevMessage() { MediaSessionId = mediaSessionId }, app.TransportId);
            return r?.Status?.FirstOrDefault();
        }

    }
}

