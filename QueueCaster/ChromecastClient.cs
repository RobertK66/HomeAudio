using Microsoft.Extensions.DependencyInjection;
using QueueCaster.queue.models;
using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using Sharpcaster.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster {
    public class ChromecastClient :Sharpcaster.ChromecastClient {


        public ChromecastClient(IServiceCollection serviceCollection) :base(serviceCollection){
        }

        public static ChromecastClient CreateNewChromecastClient() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IChromecastChannel, ConnectionChannel>();
            serviceCollection.AddTransient<IChromecastChannel, HeartbeatChannel>();
            serviceCollection.AddTransient<IChromecastChannel, ReceiverChannel>();
            serviceCollection.AddTransient<IChromecastChannel, QueueMediaChannel>();
            var messageInterfaceType = typeof(IMessage);
            foreach (var type in (from t in typeof(IConnectionChannel).GetTypeInfo().Assembly.GetTypes()
                                  where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                  select t)) {
                if (!type.Name.Equals("MediaStatusMessage")) {  // Skip this Message. We replace it with our Version including queue info
                    serviceCollection.AddTransient(messageInterfaceType, type);
                    Console.WriteLine("***** " + type.Name);
                }
            }

            // Add our own Message Classes handling the queued item lists in MediaStatusMessage and others
            foreach (var type in (from t in typeof(QueueItem).GetTypeInfo().Assembly.GetTypes()
                                  where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                  select t)) {
                serviceCollection.AddTransient(messageInterfaceType, type);
                Console.WriteLine("+++++ " + type.Name);
            }
            return new ChromecastClient(serviceCollection);
        }
    }
}
