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
                    if (ni.Name.StartsWith("WLAN")) {
                        filtered.Add(ni);
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses) {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                                ipa = ip.Address;
                                Console.WriteLine(ip.Address.ToString());
                                break;
                            }
                        }


                        //foreach (var a in ni.GetIPProperties().DnsAddresses) {
                        //    if (a.AddressFamily == AddressFamily.InterNetwork) {
                        //        ipa = a;
                        //        break;
                        //    }
                        //                            }
                        break;
                    }
                }

                return filtered;
            });



            var sd = new ServiceDiscovery(mdns);
            var sp = new ServiceProfile("googlecast", "_googlecast._tcp", port);
            sp.AddProperty("fn", "Buero");
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
                    CcStub cc = new();
                    Task.Run(() => cc.ProcessClient(client));
                }

            } catch (Exception e) {
                Console.WriteLine("Error: " + e);
                Console.ReadLine();
            }

            Console.ReadKey();
        }
    }
}

    //    private async static void ProcessClient(TcpClient client) {
    //        Console.WriteLine("Connection accepted " + client.Client.SocketType );

    //        // A client has connected. Create the
    //        // SslStream using the client's network stream.
    //        SslStream sslStream = new SslStream(
    //            client.GetStream(), false);
    //        // Authenticate the server but don't require the client to authenticate.
    //        try {
    //            sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

    //            // Display the properties and settings for the authenticated stream.
    //            DisplaySecurityLevel(sslStream);
    //            DisplaySecurityServices(sslStream);
    //            DisplayCertificateInformation(sslStream);
    //            DisplayStreamProperties(sslStream);

    //            // Set timeouts for the read and write to 5 seconds.
    //            sslStream.ReadTimeout = 5000;
    //            sslStream.WriteTimeout = 5000;
    //            // Read a message from the client.
    //            Console.WriteLine("Waiting for client message...");
    //            while (true) {
    //                string messageData = await ReadMessage(sslStream);
    //                Console.WriteLine("*** Received: {0}", messageData);
    //            }
    //            // Write a message to the client.
    //            //byte[] message = Encoding.UTF8.GetBytes("Hello from the server.<EOF>");
    //            //Console.WriteLine("Sending hello message.");
    //            //sslStream.Write(message);
    //        } catch (AuthenticationException e) {
    //            Console.WriteLine("Exception: {0}", e.Message);
    //            if (e.InnerException != null) {
    //                Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
    //            }
    //            Console.WriteLine("Authentication failed - closing the connection.");
    //            sslStream.Close();
    //            client.Close();
    //            return;
    //        } finally {
    //            // The client stream will be closed with the sslStream
    //            // because we specified this behavior when creating
    //            // the sslStream.
    //            sslStream.Close();
    //            client.Close();
    //        }
    //    }
    //    static async Task<string> ReadMessage(SslStream sslStream) {
    //        // Read the  message sent by the client.
    //        // The client signals the end of the message using the
    //        // "<EOF>" marker.
    //        byte[] buffer = new byte[500];
    //        StringBuilder messageData = new StringBuilder();
    //        StringBuilder hexData = new StringBuilder();
    //        int bytes = -1;
    //        do {
    //            // Read the client's test message.
    //            bytes = await sslStream.ReadAsync(buffer, 0, buffer.Length);

    //            // Use Decoder class to convert from bytes to UTF8
    //            // in case a character spans two buffers.
    //            Decoder decoder = Encoding.ASCII.GetDecoder();
    //            char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
    //            decoder.GetChars(buffer, 0, bytes, chars, 0);
    //            messageData.Append(chars);
    //            for (int i = 0;i<bytes;i++) {
    //                hexData.Append(" " + buffer[i].ToString("X2"));
    //            }

              
    //            //Console.WriteLine(messageData);
    //            // Check for EOF or an empty message.
    //            if (messageData.ToString().IndexOf("}") != -1) {
    //                break;
    //            }
    //        } while (bytes != 0);
    //        Console.WriteLine(hexData.ToString());  
    //        return messageData.ToString();
    //    }
    //    static void DisplaySecurityLevel(SslStream stream) {
    //        Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
    //        Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
    //        Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
    //        Console.WriteLine("Protocol: {0}", stream.SslProtocol);
    //    }
    //    static void DisplaySecurityServices(SslStream stream) {
    //        Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
    //        Console.WriteLine("IsSigned: {0}", stream.IsSigned);
    //        Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
    //        Console.WriteLine("Is mutually authenticated: {0}", stream.IsMutuallyAuthenticated);
    //    }
    //    static void DisplayStreamProperties(SslStream stream) {
    //        Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
    //        Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
    //    }
    //    static void DisplayCertificateInformation(SslStream stream) {
    //        Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

    //        X509Certificate localCertificate = stream.LocalCertificate;
    //        if (stream.LocalCertificate != null) {
    //            Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
    //                localCertificate.Subject,
    //                localCertificate.GetEffectiveDateString(),
    //                localCertificate.GetExpirationDateString());
    //        } else {
    //            Console.WriteLine("Local certificate is null.");
    //        }
    //        // Display the properties of the client's certificate.
    //        X509Certificate remoteCertificate = stream.RemoteCertificate;
    //        if (stream.RemoteCertificate != null) {
    //            Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
    //                remoteCertificate.Subject,
    //                remoteCertificate.GetEffectiveDateString(),
    //                remoteCertificate.GetExpirationDateString());
    //        } else {
    //            Console.WriteLine("Remote certificate is null.");
    //        }
    //    }
    //    private static void DisplayUsage() {
    //        Console.WriteLine("To start the server specify:");
    //        Console.WriteLine("serverSync certificateFile.cer");
    //        Environment.Exit(1);
    //    }
       
    //}




