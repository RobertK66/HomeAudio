using QueueCaster.queue.messages;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages.Media;
using Sharpcaster.Models.ChromecastStatus;
using Sharpcaster.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster {
    public  class QueueMediaChannel : MediaChannel {

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

