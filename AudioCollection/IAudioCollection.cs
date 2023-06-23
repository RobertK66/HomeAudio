using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollection {



    //public interface IAudioCollection {
    //    (string name, string url) GetRadioStation(int stationIdx);
    //    (string name, string url) GetRadioStation(string stationName);
    //    List<(string url, string name)> GetCdTracks(int albumIdx);
    //    List<(string url,  string name)> GetCdTracks(string cdid);

    //    List<(string name, List<(string url, string name)> tracks, string artist, string cdid)> GetAllAlbums();
    //    List<(string name, string url)> GetAllStations();
    //}


    public interface IAudioEntry {
        String Name { get; set; }
        String? ContentUrl { get; set; }            // If this is not null -> its a webradio, or track
        List<IAudioEntry>? AudioTracks { get; }     // if Content Url is null this contains the Tracks of a named CD (container).
    }


    public interface IAudioTab {
        public int Cols { get; set; }
        public int Rows { get; set; }
        public int CellSize { get; set; }
        public String TabName { get; set; }
        public List<IAudioEntry> GetAudioEntries();

    }

    public interface ITabedAudioCollection {
         List<IAudioTab> GetAllTabs();
    }

   
}
