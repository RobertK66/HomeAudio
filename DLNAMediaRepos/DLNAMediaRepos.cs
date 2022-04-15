using ISSDP.UPnP.PCL.Interfaces.Model;
using ISSDP.UPnP.PCL.Interfaces.Service;
using SSDP.UPnP.PCL.Service;
using ISSDP.UPnP.PCL.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SSDP.UPnP.PCL.Model;
using static SSDP.UPnP.PCL.Helper.Constants;
using System.Globalization;

namespace DLNAMediaRepos {
    public class DLNAMediaRepository : IMediaRepository<string> {
        private IControlPoint _controlPoint;
        private IPAddress _controlPointLocalIp1;

        public DLNAMediaRepository() {
            _controlPointLocalIp1 = GetBestGuessLocalIPAddress();
            System.Console.WriteLine($"IP Address: {_controlPointLocalIp1.ToString()}");


        }


        public async Task SerarchDevices() {


            var cts = new CancellationTokenSource();

            await StartAsync(cts.Token);

         //--   System.Console.WriteLine("Press any key to end search.");

         //   --System.Console.ReadKey();

         //  -- cts.Cancel();


         //   --System.Console.ReadKey();

        }

        private async Task StartAsync(CancellationToken ct) {
            await StartControlPointListeningAsync(ct);
        }

        private async Task StartControlPointListeningAsync(CancellationToken ct) {
            _controlPoint = new ControlPoint(_controlPointLocalIp1);

            _controlPoint.Start(ct);

            ListenToNotify();

            ListenToMSearchResponse(ct);

            await StartMSearchRequestMulticastAsync();
        }


        private void ListenToNotify() {
            var counter = 0;

            var observerNotify = _controlPoint.NotifyObservable();

            var disposableNotify = observerNotify
                .Subscribe(
                    n => {
                        counter++;
                        System.Console.BackgroundColor = ConsoleColor.DarkBlue;
                        System.Console.ForegroundColor = ConsoleColor.White;
                        System.Console.WriteLine($"---### Control Point Received a NOTIFY - #{counter} ###---");
                        System.Console.ResetColor();
                        System.Console.WriteLine($"{n?.NotifyTransportType.ToString()}");
                        System.Console.WriteLine($"From: {n?.HOST}");
                        System.Console.WriteLine($"Location: {n?.Location?.AbsoluteUri}");
                        System.Console.WriteLine($"Cache-Control: max-age = {n.CacheControl}");
                        System.Console.WriteLine($"Server: " +
                                                 $"{n?.Server?.OperatingSystem}/{n?.Server?.OperatingSystemVersion} " +
                                                 $"UPNP/" +
                                                 $"{n?.Server?.UpnpMajorVersion}.{n?.Server?.UpnpMinorVersion}" +
                                                 $" " +
                                                 $"{n?.Server?.ProductName}/{n?.Server?.ProductVersion}" +
                                                 $" - ({n?.Server?.FullString})");
                        System.Console.WriteLine($"NT: {n?.NT}");
                        System.Console.WriteLine($"NTS: {n?.NTS}");
                        System.Console.WriteLine($"USN: {n?.USN?.ToString()}");

                        if (n.BOOTID > 0) {
                            System.Console.WriteLine($"BOOTID: {n.BOOTID}");
                        }

                        System.Console.WriteLine($"CONFIGID: {n.CONFIGID}");

                        System.Console.WriteLine($"NEXTBOOTID: {n.NEXTBOOTID}");
                        System.Console.WriteLine($"SEARCHPORT: {n.SEARCHPORT}");
                        System.Console.WriteLine($"SECURELOCATION: {n.SECURELOCATION}");

                        if (n.Headers.Any()) {
                            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                            System.Console.WriteLine($"Additional Headers: {n.Headers.Count}");
                            foreach (var header in n.Headers) {
                                System.Console.WriteLine($"{header.Key}: {header.Value}; ");
                            }

                            System.Console.ResetColor();
                        }

                        System.Console.WriteLine($"Is UPnP 2.0 compliant: {n.IsUuidUpnp2Compliant}");

                        if (n.HasParsingError) {
                            System.Console.WriteLine($"Parsing errors: {n.HasParsingError}");
                        }

                        System.Console.WriteLine();
                    });
        }

        private void ListenToMSearchResponse(CancellationToken ct) {
            var mSearchResObs = _controlPoint.MSearchResponseObservable();

            var counter = 0;

            var disposableMSearchresponse = mSearchResObs
                .Subscribe(
                    res => {
                        counter++;
                        System.Console.BackgroundColor = ConsoleColor.DarkBlue;
                        System.Console.ForegroundColor = ConsoleColor.White;
                        System.Console.WriteLine($"---### Control Point Received a  M-SEARCH RESPONSE #{counter} ###---");
                        System.Console.ResetColor();
                        System.Console.WriteLine($"{res?.TransportType.ToString()}");
                        System.Console.WriteLine($"Status code: {res.StatusCode} {res.ResponseReason}");
                        System.Console.WriteLine($"Location: {res?.Location?.AbsoluteUri}");
                        System.Console.WriteLine($"Date: {res.Date.ToString(CultureInfo.CurrentCulture)}");
                        System.Console.WriteLine($"Cache-Control: max-age = {res.CacheControl}");
                        System.Console.WriteLine($"Server: " +
                                                 $"{res?.Server?.OperatingSystem}/{res?.Server?.OperatingSystemVersion} " +
                                                 $"UPNP/" +
                                                 $"{res?.Server?.UpnpMajorVersion}.{res?.Server?.UpnpMinorVersion}" +
                                                 $" " +
                                                 $"{res?.Server?.ProductName}/{res?.Server?.ProductVersion}" +
                                                 $" - ({res?.Server?.FullString})");
                        System.Console.WriteLine($"ST: {res?.ST?.STString}");
                        System.Console.WriteLine($"USN: {res.USN?.USNString} {res.USN?.TypeName} {res.USN?.Version} {res.USN?.EntityType}");
                        System.Console.WriteLine($"BOOTID.UPNP.ORG: {res?.BOOTID}");
                        System.Console.WriteLine($"CONFIGID.UPNP.ORG: {res?.CONFIGID}");
                        System.Console.WriteLine($"SEARCHPORT.UPNP.ORG: {res?.SEARCHPORT}");
                        System.Console.WriteLine($"SECURELOCATION: {res?.SECURELOCATION}");

                        if (res?.Headers?.Any() ?? false) {
                            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                            System.Console.WriteLine($"Additional Headers: {res.Headers?.Count}");
                            foreach (var header in res.Headers) {
                                System.Console.WriteLine($"{header.Key}: {header.Value}; ");
                            }

                            System.Console.ResetColor();
                        }

                        if (res.HasParsingError) {
                            System.Console.WriteLine($"Parsing errors: {res.HasParsingError}");
                        }

                        System.Console.WriteLine();
                    });
        }


        private async Task StartMSearchRequestMulticastAsync() {
            var mSearchMessage = new MSearch {
                TransportType = TransportType.Multicast,
                CPFN = "TestXamarin",

                Name = UdpSSDPMultiCastAddress,
                Port = UdpSSDPMulticastPort,
                MX = TimeSpan.FromSeconds(5),
                TCPPORT = TcpResponseListenerPort.ToString(),
                //ST = new ST("urn:myharmony-com:device:harmony:1"),
                ST = new ST {
                    StSearchType = STType.All
                },
                //ST = new ST
                //{
                //    STtype  = STtype.ServiceType,
                //    Type = "SwitchPower",
                //    Version = "1",
                //    HasDomain = false
                //},
                //ST = new ST
                //{
                //    StSearchType = STSearchType.DomainDeviceSearch,
                //    Domain = "myharmony-com", 
                //    DeviceType = "harmony",
                //    Version = "1",
                //    //STtype = STtype.DeviceType,
                //    ////DeviceUUID = "myharmony-com:device:harmony:1",
                //    //Type = "harmony",
                //    //Version = "1",
                //    //HasDomain = true,
                //    //DomainName = "myharmony-com"
                //},

                UserAgent = new UserAgent {
                    OperatingSystem = "Windows",
                    OperatingSystemVersion = "10.0",
                    ProductName = "SSDP.UPNP.PCL",
                    ProductVersion = "0.9",
                    UpnpMajorVersion = "2",
                    UpnpMinorVersion = "0",
                }
            };

            await _controlPoint.SendMSearchAsync(mSearchMessage, _controlPointLocalIp1);
        }


        public List<(string, string)> GetCdTracks(int playIdx) {
            List<(string,string)> trackURIs = new List<(string, string)>();
            return trackURIs;
        }

        public (string, string) GetRadioStation(int playIdx) {
            return ("https://orf-live.ors-shoutcast.at/oe1-q2a", "radio x");
        }


        //https://stackoverflow.com/a/9100336/4140832
        public static IPAddress GetBestGuessLocalIPAddress() {
            var addresses = new List<IPAddress>();
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces) {
                // check whether turned on
                if (network.OperationalStatus == OperationalStatus.Up) {
                    if (network.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

                    var properties = network.GetIPProperties();

                    if (properties.GatewayAddresses.Count > 0) {
                        var good = false;
                        foreach (var gInfo in properties.GatewayAddresses) {
                            //not a true gateaway (VmWare Lan)
                            if (!gInfo.Address.ToString().Equals("0.0.0.0")) {
                                good = true;
                                break;
                            }
                        }
                        if (!good) {
                            continue;
                        }
                    } else {
                        continue;
                    }

                    foreach (var address in properties.UnicastAddresses) {
                        // We're only interested in IPv4 addresses for now       
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork) continue;

                        // Ignore loopback addresses (e.g., 127.0.0.1)    
                        if (IPAddress.IsLoopback(address.Address)) continue;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                            if (!address.IsDnsEligible) continue;

                            if (address.IsTransient) continue;
                        }




                        addresses.Add(address.Address);
                    }
                }
            }

            return addresses.FirstOrDefault();
        }
    }

    internal class UserAgent : IUserAgent {
        public string FullString { get; set; }

        public string OperatingSystem { get; set; }

        public string OperatingSystemVersion { get; set; }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }

        public string UpnpMajorVersion { get; set; }

        public string UpnpMinorVersion { get; set; }

        public bool IsUpnp2 { get; set; }
    }


    internal class MSearch : IMSearchRequest {
        public bool InvalidRequest { get; } = false;
        public bool HasParsingError { get; internal set; }
        public string Name { get; internal set; }
        public int Port { get; internal set; }
        public IDictionary<string, string> Headers { get; internal set; }
        public TransportType TransportType { get; internal set; }
        public string MAN { get; internal set; }
        public string HOST { get; internal set; }
        public TimeSpan MX { get; internal set; }
        public IST ST { get; internal set; }
        public IUserAgent UserAgent { get; internal set; }
        public string CPFN { get; internal set; }
        public string CPUUID { get; internal set; }
        public int SEARCHPORT { get; internal set; }
        public string TCPPORT { get; internal set; }
        public IPEndPoint LocalIpEndPoint { get; internal set; }
        public IPEndPoint RemoteIpEndPoint { get; internal set; }
    }

}






    // For this test to work you most likely need to stop the SSDP Discovery service on Windows
    // If you don't stop the SSDP Windows Service, the service will intercept the UPnP multicasts and consequently nothing will show up in the console. 

 

