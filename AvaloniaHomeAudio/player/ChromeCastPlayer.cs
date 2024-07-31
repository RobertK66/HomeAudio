using AudioCollectionApi.api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaHomeAudio.player {
    public class ChromeCastPlayer : IChromeCastPlayer {
        private readonly ILogger _logger;

        public ChromeCastPlayer(ILogger<ChromeCastPlayer> logger) {
            _logger = logger;
        }

        public void Play(IMedia media) {
            _logger.LogInformation("Play: "+ media.Name);
        }
    }
}
