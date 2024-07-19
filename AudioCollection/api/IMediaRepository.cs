﻿using AudioCollectionApi.api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi
{
    public interface IMediaRepository {
        //void AddCdRepos(string collectionid, string path);
        //void AddRadioRepos(string collectionid, string path);

        //void LoadAll(object PersitenceContext);
        Task LoadAllAsync(object PersitenceContext);
        //Task<int> LoadRadioStationsAsync();

        ObservableCollection<Cd> GetCdRepository(string collectionid);
        ObservableCollection<NamedUrl> GetRadioRepository(string collectionid);
        ObservableCollection<MediaCategory> GetRadioCategories();
        ObservableCollection<MediaCategory> GetCdCategories();
    }

    public interface IMediaRepository2 {
        Task LoadAllAsync(object PersitenceContext);
        ObservableCollection<MediaCategory> GetCategories();
        ObservableCollection<IMedia> GetMediaRepository(string categoryId);
    }

}
