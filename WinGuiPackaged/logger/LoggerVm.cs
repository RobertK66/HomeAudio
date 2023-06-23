using System;
using System.Collections.ObjectModel;
using WinGuiPackaged.model;

namespace WinGuiPackaged.logger {
    public class LoggerVm {

        private ObservableCollection<String> entries = new();
        public ObservableCollection<String> Entries { get { return entries; } }
        

        internal void Add(string v) {
            entries.Add(v);
        }
    }
}