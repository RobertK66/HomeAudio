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

        private IObservableContext<IPlayerProxy>? _myPropChangedContext;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                _myPropChangedContext?.InvokePropChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
                //PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        public void SetContext<IPlayerProxy>(IObservableContext<IPlayerProxy> myContext) {
            _myPropChangedContext = (IObservableContext<AudioCollectionApi.api.IPlayerProxy>?)(myContext);
        }

        public void VolumeUp() {
            _client.VolumeUp(Id);
        }

        public void VolumeDown() {
            _client.VolumeDown(Id);
        }

        public async Task<bool> TryConnectAsync(string appId) {
            await _client.TryConnectAsync(this);
            return true;
        }

        public void Disconnect() {
            IsConnected = false;
        }

        public void PlayCd(IMedia cd) {
            _client.PlayCd(cd);
        }

        public void PlayRadio(IMedia radio) {
            _client.PlayRadio(radio);
        }

        public void Stop() {
            _ = _client.StopPlayAsync(Id);
        }
        public void Play() {
            _ = _client.PlayAsync(Id);
        }

     
    }
}
