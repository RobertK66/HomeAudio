using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLNAMediaRepos {



    public interface IMediaRepository {
        (string url, string name) GetRadioStation(int playIdx);
        List<(string url, string name)> GetCdTracks(int playIdx);

        List<(string name, List<(string url, string name)> tracks, string artist)> GetAllAlbums();
        List<(string url, string name)> GetAllStations();
    }


   
}
