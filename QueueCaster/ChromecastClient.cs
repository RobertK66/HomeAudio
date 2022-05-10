using Microsoft.Extensions.DependencyInjection;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages;
using Sharpcaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster {
    public class ChromecastClient :Sharpcaster.ChromecastClient {

        public ChromecastClient(IServiceCollection serviceCollection) : base(serviceCollection) {
        }

        public ChromecastClient(List<IChromecastChannel> channelsToUse, List<Assembly> customMessageTypeAssemblies, IConsoleWrapper? consoleWrapper) :
            base(channelsToUse, customMessageTypeAssemblies, consoleWrapper) {
        }

        public static ChromecastClient CreateQueueCasterClient(IConsoleWrapper? conWrapper = null) {
            var customMessages      = new List<Assembly>();
            var customCcChannels    = new List<IChromecastChannel>();

            customMessages.Add(typeof(QueueItem).GetTypeInfo().Assembly);
            customCcChannels.Add(new ConnectionChannel());
            customCcChannels.Add(new HeartbeatChannel());
            customCcChannels.Add(new ReceiverChannel());
            customCcChannels.Add(new QueueMediaChannel());

            return new ChromecastClient(customCcChannels, customMessages, conWrapper);
        }

    }
}
