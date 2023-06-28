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

    public class MediaStatusChangedEventArgs : EventArgs {
        public List<MediaStatus> Status { get; set; }

        public MediaStatusChangedEventArgs(IEnumerable<MediaStatus> status) {
            this.Status = status.ToList();
        }
    }

    public  class QueueMediaChannel : MediaChannel {

        public event EventHandler<MediaStatusChangedEventArgs>? QueueMediaStatusChanged;

        // The Media Channel implements StatusChannel<MediaStatusMessage, IEnumerable<MediaStatus>>, IStatusChannel<IEnumerable<MediaStatus>>,
        // with the original Sharpcaster.MediaStatus. In order to get the changed event of media status with our own class we have to 'translate'/overwrite the 
        // Event Handler implemented in StatusChannel here.
        public override Task OnMessageReceivedAsync(IMessage message) {
            MediaStatusMessage? msm = message as MediaStatusMessage;
            if (msm != null) {
                QueueMediaStatusChanged?.Invoke(this, new MediaStatusChangedEventArgs(msm.Status));
            }
            return Task.CompletedTask;
        }


        // overwrite the base method for Load media beacuse we have another MediaStatus class registered as is used there!
        // -> cast exception!
        public new async Task<MediaStatus?> LoadAsync(Media media, bool autoPlay = true) {
            MediaStatus? returnVal = null;
            var status = Client.GetChromecastStatus();
            var app = status?.Applications?.FirstOrDefault();
            if (app != null) {
                var r = await SendAsync<MediaStatusMessage>(new LoadMessage() { SessionId = app.SessionId, Media = media, AutoPlay = autoPlay }, app.TransportId);
                returnVal = r?.Status?.FirstOrDefault();
            }
            return returnVal;
        }

        
        // Media Channel methods to handle Queue
        public async Task<MediaStatus?> GetStatusAsync() {
            MediaStatus? returnVal = null;
            var status = Client.GetChromecastStatus();
            var app = status?.Applications?.FirstOrDefault();
            if (app != null) {
                var r = await SendAsync<MediaStatusMessage>(new GetStatusMessage(), app.TransportId);
                returnVal = r?.Status?.FirstOrDefault();
            }
            return returnVal;
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

