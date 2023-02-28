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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;

namespace QueueCaster {
    public class ChromecastClient :Sharpcaster.ChromecastClient {

        public ChromecastClient(IServiceCollection serviceCollection) : base(serviceCollection) {
        }

        public static ChromecastClient CreateQueueCasterClient(ILoggerFactory? loggerFactory) {
            IServiceCollection serviceCollection = new ServiceCollection();
            if (loggerFactory != null) {
                serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            }

            var customMessages      = new List<Assembly>();
            var customCcChannels    = new List<IChromecastChannel>();
            customMessages.Add(typeof(QueueItem).GetTypeInfo().Assembly);

            serviceCollection.AddTransient<IChromecastChannel, ConnectionChannel>();
            serviceCollection.AddTransient<IChromecastChannel, HeartbeatChannel>();
            serviceCollection.AddTransient<IChromecastChannel, ReceiverChannel>();
            serviceCollection.AddTransient<IChromecastChannel, QueueMediaChannel>();

            var messageInterfaceType = typeof(IMessage);
            List<Type> messageTypes = new List<Type>();

            // first add our own IMessage classes
            foreach (var type in (from t in typeof(QueueItem).GetTypeInfo().Assembly.GetTypes()
                                  where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                  select t)) {
                messageTypes.Add(type);
            }
            // then add all from basis impl wich are not there yet. (So you can 'overwrite' existing ones!)
            foreach (var type in (from t in typeof(IConnectionChannel).GetTypeInfo().Assembly.GetTypes()
                                  where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                  select t)) {
                if (messageTypes.Where(q => q.Name == type.Name).Count()==0) {
                    messageTypes.Add(type);
                }
            }
            foreach(var type in messageTypes) {
                serviceCollection.AddTransient(messageInterfaceType, type);
            }

            return new ChromecastClient(serviceCollection);
        }

    }
}
