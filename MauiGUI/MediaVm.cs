using AudioCollectionApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiGUI {
    public  class MediaVm : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private ObservableCollection<MediaCategory> _cdcat = new ();
        public ObservableCollection<MediaCategory> cdcat { get { return _cdcat; } set { if (_cdcat != value) { _cdcat = value; RaisePropertyChanged(); } } }


        public MediaVm() {
            cdcat.Add(new MediaCategory("maid") { Name="lkjkljkljkl"});
        }

    }
}
