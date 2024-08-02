using AudioCollectionApi.api;
using System.Dynamic;

namespace AvaloniaHomeAudio.player
{
    public interface IChromeCastPlayer {
        string PlayerStatus { get; }
        void Play(IMedia media);
    }
}