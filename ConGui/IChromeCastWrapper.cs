using System;
using Sharpcaster.Models.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui {
    public interface IChromeCastWrapper {
        event EventHandler? StatusChanged;

        Task Pause();
        public Task<MediaStatus?> PlayCdTracks(List<(string url, string name)> tracks);
        public Task<MediaStatus?> PlayLive(string url, string? name = null);
        Task PlayNext();
        Task PlayPrev();
        Task Shutdown();
        Task VolumeDown();
        Task VolumeUp();
    }
}
