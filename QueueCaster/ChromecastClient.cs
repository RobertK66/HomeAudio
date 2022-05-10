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

        public static ChromecastClient CreateNewChromecastClient(IConsoleWrapper? conWrapper = null) {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IChromecastChannel, ConnectionChannel>();
            serviceCollection.AddTransient<IChromecastChannel, HeartbeatChannel>();
            serviceCollection.AddTransient<IChromecastChannel, ReceiverChannel>();
            serviceCollection.AddTransient<IChromecastChannel, QueueMediaChannel>();
            if (conWrapper == null) {
                serviceCollection.AddTransient<IConsoleWrapper, ConsoleWrapper>();
            } else {
                serviceCollection.AddSingleton<IConsoleWrapper>(conWrapper);
            }

            var customMessages = new List<Assembly>();
            customMessages.Add(typeof(QueueItem).GetTypeInfo().Assembly);

            List<Type> messageImplTypes = ListAllMessageClasses(customMessages);

            foreach (var item in messageImplTypes) {
                serviceCollection.AddTransient(typeof(IMessage), item);
            }
            return new ChromecastClient(serviceCollection);
        }

        private static List<Type> ListAllMessageClasses(List<Assembly> messageAssemblies) {
            Dictionary<string, Type> allMessagesDictionary = new();

            // add the base library in front of custom assemblies -> last one wins!
            messageAssemblies.Insert(0, typeof(IConnectionChannel).GetTypeInfo().Assembly);

            var messageInterfaceType = typeof(IMessage);
            foreach (Assembly assembly in messageAssemblies) {
                foreach (var type in (from t in assembly.GetTypes()
                                      where t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && messageInterfaceType.IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ReceptionMessageAttribute>() != null
                                      select t)) {
                    if (allMessagesDictionary.ContainsKey(type.Name)) {
                        // Overwrite the base implementation with new one
                        allMessagesDictionary.Remove(type.Name);
                    }
                    allMessagesDictionary.Add(type.Name, type);
                }
            }

            return allMessagesDictionary.Values.ToList();
            
        }
    }
}
