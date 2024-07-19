using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TestServers {
    public class CcStub {
        static X509Certificate? serverCertificate = null;

        private SslStream? sslStream;
        private bool processing = false;

        public async void ProcessClient(TcpClient client) {
            Console.WriteLine("process client.");
            if (!processing) {
                processing = true;
                Console.WriteLine("Connection accepted.");
                if (serverCertificate == null) {
                    X509Store store = new X509Store(StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly);
                    serverCertificate = store.Certificates[0];
                }

                // A client has connected. Create the
                // SslStream using the client's network stream.
                sslStream = new SslStream(client.GetStream(), false);

                // Authenticate the server but don't require the client to authenticate.
                try {
                    sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                    // Display the properties and settings for the authenticated stream.
                    DisplaySecurityLevel(sslStream);
                    DisplaySecurityServices(sslStream);
                    DisplayCertificateInformation(sslStream);
                    DisplayStreamProperties(sslStream);

                    // Set timeouts for the read and write to 5 seconds.
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;

                    // Read a message from the client.
                    Console.WriteLine("Waiting for client message...");
                    while (true) {
                        ProtoMessage messageData = await ReadMessage(sslStream);
                        Console.WriteLine("*** Received: {0}", messageData);
                        var msgjson = messageData.GetField(6)?.ToString();
                        if (msgjson != null) {
                            JsonNode ccMessage = JsonNode.Parse(msgjson)!;
                            JsonNode reqId = ccMessage!["requestId"]!;
                            JsonNode type = ccMessage!["type"]!;
                            if (type.AsValue().GetValue<String>().StartsWith("GET_STATUS")) {
                                await SendStatus(reqId.AsValue().GetValue<int>(), messageData);
                            } else if (type.AsValue().GetValue<String>().StartsWith("LAUNCH")) {
                                JsonNode appId = ccMessage!["appId"]!;
                                await SendStatus(reqId.AsValue().GetValue<int>(), messageData, appId.AsValue().GetValue<string>());
                            }

                        }
                    }
                } catch (AuthenticationException e) {
                    Console.WriteLine("Exception: {0}", e.Message);
                    if (e.InnerException != null) {
                        Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                    }
                    Console.WriteLine("Authentication failed - closing the connection.");
                    sslStream.Close();
                    client.Close();
                    return;
                } finally {
                    // The client stream will be closed with the sslStream
                    // because we specified this behavior when creating
                    // the sslStream.
                    sslStream.Close();
                    client.Close();
                    processing = false;
                }
            }
        }
        private async Task<ProtoMessage> ReadMessage(SslStream sslStream) {
            Int32 packageSize;
             
            byte[] buffer = new byte[500];
            int cnt = -1;
          
            byte[] bytes = { 0, 0, 0, 0 };
            cnt = await sslStream.ReadAsync(bytes, 0, 4);
            Array.Reverse(bytes);
            packageSize = BitConverter.ToInt32(bytes, 0);

            cnt = await sslStream.ReadAsync(buffer, 0, packageSize);
            ProtoMessage retVal = new ProtoMessage();
            for (int i = 0; i < cnt; i++) {
                retVal.ProcessByte(buffer[i]);
            }

            return retVal;
        }

       
        static void DisplaySecurityLevel(SslStream stream) {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream) {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
            Console.WriteLine("Is mutually authenticated: {0}", stream.IsMutuallyAuthenticated);
        }
        static void DisplayStreamProperties(SslStream stream) {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream) {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate? localCertificate = stream?.LocalCertificate;
            if (stream?.LocalCertificate != null) {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate?.Subject,
                    localCertificate?.GetEffectiveDateString(),
                    localCertificate?.GetExpirationDateString());
            } else {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate? remoteCertificate = stream?.RemoteCertificate;
            if (stream?.RemoteCertificate != null) {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate?.Subject,
                    remoteCertificate?.GetEffectiveDateString(),
                    remoteCertificate?.GetExpirationDateString());
            } else {
                Console.WriteLine("Remote certificate is null.");
            }
        }
        private static void DisplayUsage() {
            Console.WriteLine("To start the server specify:");
            Console.WriteLine("serverSync certificateFile.cer");
            Environment.Exit(1);
        }

        async Task SendStatus(int reqId, ProtoMessage msg, string? AppId = null) {

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"requestId\": " + reqId + ",");
            sb.Append("\"status\": { \"applications\": [ { ");
            if (AppId != null) {
                sb.Append("\"appId\": \""+AppId+"\", \"appType\": \"ANDROID_TV\", \"displayName\": \"YouTube\",");
            } else {
                sb.Append("\"appId\": \"2C6A6E3D\", \"appType\": \"ANDROID_TV\", \"displayName\": \"YouTube\",");
            }
            sb.Append("\"isIdleScreen\": false, \"launchedFromCloud\": false, \"namespaces\": [ ");
            sb.Append("{ \"name\": \"urn:x-cast:com.google.cast.media\" }, { \"name\": \"urn:x-cast:com.google.cast.system\" },");
            sb.Append("{ \"name\": \"urn:x-cast:com.google.cast.cac\" }, { \"name\": \"urn:x-cast:com.google.youtube.mdx\" } ],");
            sb.Append("\"sessionId\": \"0167f70a-6430-45ab-9714-fd2f09d70b2b\", \"statusText\": \"Youtube\", ");
            sb.Append("\"transportId\": \"0167f70a-6430-45ab-9714-fd2f09d70b2b\", \"universalAppId\": \"233637DE\" } ], ");
            sb.Append("\"isActiveInput\": true, \"isStandBy\": false, \"userEq\": { }, \"volume\": { \"controlType\": \"master\", \"level\": 0.10000000149011612, \"muted\": false, \"stepInterval\": 0.009999999776482582 } }");
            sb.Append(", \"type\": \"RECEIVER_STATUS\"  }  ");
            
            msg.SetField(6, sb.ToString());
            Console.WriteLine("Send: {0}", msg);
            byte[] buffer = msg.GetAsBytes();
            int len = buffer.Length;
            byte[] lenB = BitConverter.GetBytes(len);
            Array.Reverse(lenB);
            if (sslStream != null) {
                await sslStream.WriteAsync(lenB);
                await sslStream.WriteAsync(buffer);
            }

        }
    }
}

