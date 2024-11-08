using AudioCollectionApi.api;
using AvaloniaHomeAudio.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaHomeAudio {

    public class DummyPlayer : IPlayerProxy {
        public string Name => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public string Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? MediaStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Disconnect() {
            throw new NotImplementedException();
        }

        public void Play() {
            throw new NotImplementedException();
        }

        public void PlayCd(IMedia cd) {
            throw new NotImplementedException();
        }

        public void PlayRadio(IMedia radio) {
            throw new NotImplementedException();
        }

        public void SetContext(IObservableContext myContext) {
            throw new NotImplementedException();
        }

        public void Stop() {
            throw new NotImplementedException();
        }

        public Task<bool> TryConnectAsync(string appId) {
            throw new NotImplementedException();
        }

        public void VolumeDown() {
            throw new NotImplementedException();
        }

        public void VolumeUp() {
            throw new NotImplementedException();
        }
    }

    public class DtMainViewModel : MainViewModel {
        public DtMainViewModel(ILogger<MainViewModel>? logger, IMediaRepository? repos, IPlayerRepository playerRepos) : base(null, null, null) {
            Player = new DummyPlayer();
        }

        public override Task LoadReposAsync() {
            return Task.CompletedTask;
        }
    }
}
