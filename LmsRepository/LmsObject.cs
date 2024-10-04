using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LmsRepositiory {
    public class LmsObject :IMedia  {
        public String Id { get { return _id; } }
        public String Name { get { return _name; }  }


        private bool _isCollection = false;
        public bool IsCollection { get { return _isCollection; } }

        public IList<IMedia> Content => new List<IMedia>();
            
        public string? ContentUrl => Id;

        private String _id = "";
        private String _name = "";
        //private String _status = "";
        //private int _volume = -1;
        //private string? _mediaStatus;
        //private string? _appId;
        //private bool _isConnected = false;
        
        //private bool _isOn = false;

        //public event PropertyChangedEventHandler? PropertyChanged;
        //public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
        //    if (PropertyChanged != null) {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        //public String Status { get { return _status; } set { _status = value; RaisePropertyChanged(); } }
        //public int Volume { get { return _volume; } set { _volume = value; RaisePropertyChanged(); } }
        //public String? MediaStatus { get { return _mediaStatus; } set { _mediaStatus = value; RaisePropertyChanged(); } }
        //public String? AppId { get { return _appId; } set { _appId = value; RaisePropertyChanged(); } }
        //public bool IsConnected { get { return _isConnected; } set { _isConnected = value; RaisePropertyChanged(); } }

        //public bool IsOn { get { return _isOn; } set { _isOn = value; RaisePropertyChanged(); } }

        public LmsObject(String id, String name, bool isCollection = false) {
            _id = id;
            _name = name;
            _isCollection = isCollection;
        }

    }
}
