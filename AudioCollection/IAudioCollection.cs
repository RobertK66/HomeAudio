using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollection {



    public interface IAudioCollection {
        (string name, string url) GetRadioStation(int stationIdx);
        (string name, string url) GetRadioStation(string stationName);
        List<(string url, string name)> GetCdTracks(int albumIdx);
        List<(string url,  string name)> GetCdTracks(string cdid);

        List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> GetAllAlbums();
        List<(string name, string url)> GetAllStations();
    }


   
}
