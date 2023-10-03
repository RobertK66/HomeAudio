using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi
{
    public interface IMediaRepository
    {
        //void AddCdRepos(string collectionid, string path);
        //void AddRadioRepos(string collectionid, string path);

        void LoadAll(object PersitenceContext);

        ObservableCollection<Cd> GetCdRepository(string collectionid);
        ObservableCollection<NamedUrl> GetRadioRepository(string collectionid);
        IEnumerable<MediaCategory> GetRadioCategories();
        IEnumerable<MediaCategory> GetCdCategories();
    }
}
