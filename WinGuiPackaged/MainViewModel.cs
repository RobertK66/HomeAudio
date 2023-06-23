using Microsoft.Extensions.Logging;
using Sharpcaster.Interfaces;
using Sharpcaster.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinGuiPackaged.model;

namespace WinGuiPackaged
{

    public class MainViewModel : IRadioViewModel, ICdViewModel
    {
        public ILoggerFactory LoggerFactory { get; set; }
        public logger.LoggerVm LogWindowViewModel { get; set; }


        private ObservableCollection<Cd> cds = new ();
        public ObservableCollection<Cd> Cds { get { return cds; } }
        public Cd SelectedCd { get; set; } = null;


        private ObservableCollection<NamedUrl> radios = new ();
        public ObservableCollection<NamedUrl> WebRadios { get { return radios; } }
        public NamedUrl SelectedRadio { get; set; } = null;


        private ObservableCollection<ChromecastReceiver> receiver = new ();
        public ObservableCollection<ChromecastReceiver> Receiver { get { return receiver; } }
        public ChromecastReceiver SelectedReceiver { get; set; } = null;


        public MainViewModel(ILoggerFactory loggerFactory, logger.LoggerVm logVm) {
            try {
                LoggerFactory = loggerFactory;
                LogWindowViewModel = logVm;
                receiver.CollectionChanged += Receiver_CollectionChanged;

                var path = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["radioRepos"];
                if (File.Exists(path)) {
                    var cont = JsonSerializer.Deserialize<List<NamedUrl>>(File.ReadAllText(path));
                    foreach (var item in cont) {
                        radios.Add(item);
                    }
                }
                path = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["cdRepos"];
                if (File.Exists(path)) {
                    var cont = JsonSerializer.Deserialize<List<Cd>>(File.ReadAllText(path));
                    foreach (var item in cont) {
                        cds.Add(item);
                    }
                }

                IChromecastLocator locator = new Sharpcaster.MdnsChromecastLocator();
                locator.ChromecastReceivedFound += Locator_ChromecastReceivedFound;
                _ = locator.FindReceiversAsync(CancellationToken.None);         // Fire the search process and wait for receiver found events!


            } catch (Exception ex) {
                // Log Error -> in GUI 
                radios.Add(new NamedUrl() { Name = "Init Error", ContentUrl = ex.Message });
            }
        }

        private void Receiver_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            foreach (var item in e.NewItems) { 
                var rec = item as ChromecastReceiver;
                if (rec != null) {
                    if (rec.Name.StartsWith("Bü")) {
                        SelectedReceiver = rec;
                    }
                }
            }
        }

        private void Locator_ChromecastReceivedFound(object sender, Sharpcaster.Models.ChromecastReceiver e) {
            Receiver.Add(e);
        }
    }
}
