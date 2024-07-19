using AudioCollectionApi.model;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi.api
{
    public interface IMediaRepository2 {
        Task LoadAllAsync(object PersitenceContext);
        ObservableCollection<MediaCategory> GetCategories();
        ObservableCollection<IMedia> GetMediaRepository(string categoryId);
    }

}
