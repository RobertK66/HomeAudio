using AudioCollectionApi.api;
using LmsRepositiory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LmsRepository
{
    public class LmsPlayer : LmsObject , IPlayerProxy, INotifyPropertyChanged {
        private LmsClientRepos _client;

        private String _status = "";
        private int _volume = -1;
        private string? _mediaStatus;
        private string? _appId;
        private bool _isConnected = false;
        private bool _isOn = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public String Status { get { return _status; } set { _status = value; RaisePropertyChanged(); } }
        public int Volume { get { return _volume; } set { _volume = value; RaisePropertyChanged(); } }
        public String? MediaStatus { get { return _mediaStatus; } set { _mediaStatus = value; RaisePropertyChanged(); } }
        public String? AppId { get { return _appId; } set { _appId = value; RaisePropertyChanged(); } }
        public bool IsConnected { get { return _isConnected; } set { _isConnected = value; RaisePropertyChanged(); } }

        public bool IsOn { get { return _isOn; } set { _isOn = value; RaisePropertyChanged(); } }


        public LmsPlayer(String id, String name, LmsClientRepos client) : base(id, name, false) {
            _client = client;
        }

        public void VolumeUp() {
            Volume =  _client.VolumeUp(Id);
        }

        public void VolumeDown() {
            Volume = _client.VolumeDown(Id);
        }

        public async Task<bool> TryConnectAsync(string appId) {
            await _client.TryConnectAsync(this);
            return true;
        }

        public void Disconnect() {
            IsConnected = false;
        }
    }
}
