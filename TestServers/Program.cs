using Common.Logging;
using Makaretu.Dns;
using Common.Logging.Simple;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TestServers {
    internal class Program {
        static X509Certificate? serverCertificate = null;


        static void Main(string[] args) {
            Console.WriteLine("Multicast DNS server");

            //// set logger factory
            //var properties = new Common.Logging.Configuration.NameValueCollection {
            //    ["level"] = "TRACE",
            //    ["showLogName"] = "true",
            //    ["showDateTime"] = "true",
            //    ["dateTimeFormat"] = "HH:mm:ss.fff"

            //};
            //LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
            // 

            IPAddress ipa = IPAddress.Parse("127.0.0.0");
            ushort port = 5010;

            var mdns = new MulticastService((li) => {
                List<NetworkInterface> filtered = new List<NetworkInterface>();
                foreach (var ni in li) {
                    Console.WriteLine(ni.Name);
                    if (ni.Name.StartsWith("WLAN")) {
                        filtered.Add(ni);
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses) {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                                ipa = ip.Address;
                                Console.WriteLine(ip.Address.ToString());
                                break;
                            }
                        }
                        break;
                    }
                }

                return filtered;
            });

            Task.Delay(2000);

            var sd = new ServiceDiscovery(mdns);
            var sp = new ServiceProfile("googlecast", "_googlecast._tcp", port, new List<IPAddress>() { ipa });
            Console.WriteLine("service profile content:" + ipa.ToString());
            sp.AddProperty("fn", "XBuero");
            sp.AddProperty("md", "MyModel");
            sp.AddProperty("ve", "my vers");
            sp.AddProperty("rs", "my status");
            sd.Advertise(sp);
            mdns.Start();
            Console.WriteLine("Multicast server started");

            try {


                Console.WriteLine("Starting TCP listener...");
                //X509Store store = new X509Store(StoreLocation.CurrentUser);
                //store.Open(OpenFlags.ReadOnly);
                //serverCertificate = store.Certificates[0];

                TcpListener listener = new TcpListener(ipa, port);

                listener.Start();

                Console.WriteLine("Server is listening on " + listener.LocalEndpoint);

                Console.WriteLine("Waiting for a connection...");

                while (true) {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("client arrived " + client.ToString());
                    //CcStub cc = new();

                    //Task.Run(() => cc.ProcessClient(client));
                }

            } catch (Exception e) {
                Console.WriteLine("Error: " + e);
                Console.ReadLine();
            }

            Console.ReadKey();
        }
    }
}

  



