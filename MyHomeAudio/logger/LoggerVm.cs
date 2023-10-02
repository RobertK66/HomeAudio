using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace MyHomeAudio.logger {
    public class LoggerVm :INotifyPropertyChanged {

        public DispatcherQueue dq { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged(string name) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private ObservableCollection<String> entries = new();
        public ObservableCollection<String> Entries { get { return entries; } }

        public string ContentText { get; set; } = "";

        internal void Add(string v) {
            dq?.TryEnqueue(() => {
                entries.Add(v);
                ContentText += v + Environment.NewLine;
                RaisePropertyChanged("ContentText");
            });

        }
    }
}