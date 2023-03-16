using QueueCaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui {
    public interface IChromeCastWrapper {
        event EventHandler? StatusChanged;

        public Task<MediaStatus?> PlayCdTracks(List<(string url, string name)> tracks);
        public Task<MediaStatus?> PlayLive(string url, string? name = null);
        Task PlayNext();
        Task PlayPrev();
        void VolumeDown();
        void VolumeUp();
    }
}
