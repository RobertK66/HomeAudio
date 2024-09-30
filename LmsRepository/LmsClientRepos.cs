using AudioCollectionApi.api;
using AudioCollectionApi.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LmsRepositiory {
    public class LmsClientRepos :IMediaRepository, IPlayerRepository {
        HttpClient client = new HttpClient();

        private ObservableCollection<MediaCategory> mediaCategories = new ObservableCollection<MediaCategory>() {
            new MediaCategory("Radios"){ Name="Radios" },
            new MediaCategory("CDs"){ Name="CDs" }
        };

        private ObservableCollection<IMedia> radioList = new ObservableCollection<IMedia>();
        private ObservableCollection<IMedia> albumList = new ObservableCollection<IMedia>();

        private Dictionary<String, ObservableCollection<IMedia>> mediaRepositories = new Dictionary<String, ObservableCollection<IMedia>>();

        public event EventHandler<IPlayerProxy>? PlayerFound;

        public string BaseUrl { get; set; } = "http://192.168.177.65:9000/";

        private ObservableCollection<IPlayerProxy> _knownPlayer = new ObservableCollection<IPlayerProxy>();
        public ObservableCollection<IPlayerProxy> KnownPlayer { get { return _knownPlayer; } }

        public LmsClientRepos() {
            mediaRepositories.Add("Radios", radioList);
            mediaRepositories.Add("CDs", albumList);
        }

        public async Task<IEnumerable<LmsObject>> GetPlayersAsync() {
            return await GetLmsObjectsAsync(new LmsJsonRequest(string.Empty, new object[] { "players", 0, 10 }), "players_loop", "playerid", "name");
        }

        public async Task<IEnumerable<LmsObject>> GetAlbumsAsync() {
            return await GetLmsObjectsAsync(new LmsJsonRequest(string.Empty, new object[] { "albums", 0, 300, "sort:album" }), "albums_loop", "id", "album");
        }

        public async Task<IEnumerable<LmsObject>> GetRadiosAsync() {
            return await GetLmsObjectsAsync(new LmsJsonRequest(string.Empty, new object[] { "favorites", "items", 0, 50 }), "loop_loop", "id", "name");
        }

        private async Task<IEnumerable<LmsObject>> GetLmsObjectsAsync(LmsJsonRequest request, string loopname, string idprop, string nameprop) {
            List<LmsObject> retVal = new List<LmsObject>();
            var url = string.Concat(BaseUrl, "jsonrpc.js");
            bool isCollection = nameprop.Equals("album");
            


            string json = JsonSerializer.Serialize(request);
            var response = await client.PostAsync(url, new StringContent(json));
            var res =  response.Content.ReadAsStringAsync().Result;

            JsonDocument jdoc = JsonDocument.Parse(res);

            var r = jdoc.RootElement.GetProperty("result");
            var p = r.GetProperty(loopname);
            foreach(var x in  p.EnumerateArray()) {
                try {
                    retVal.Add(new LmsObject(x.GetProperty(idprop).ToString(), x.GetProperty(nameprop).ToString(), isCollection));
                } catch (Exception) {
                    // TODO: logging //error out 
                }
            }
            return retVal;
        }

        public void PlayRadio(string playerid, string? id) {
            //Request: "6e:ef:54:e9:02:b0 favorites playlist play item_id:1.1<LF>"
            var request = new LmsJsonRequest(playerid, new object[] { "favorites", "playlist", "play", $"item_id:{id}" });
            var url = string.Concat(BaseUrl, "jsonrpc.js");

            string json = JsonSerializer.Serialize(request);
            var response = client.PostAsync(url, new StringContent(json)).Result;

            // no usefull content in result....
            //var res = response.Content.ReadAsStringAsync().Result;
            //JsonDocument jdoc = JsonDocument.Parse(res);
            //var r = jdoc.RootElement.GetProperty("result");


        }

        public int PlayAlbum(string playerid, string? id) {
            //Request: "a5:41:d2:cd:cd:05 playlistcontrol cmd:load album_id:22<LF>"
            var request = new LmsJsonRequest(playerid, new object[] { "playlistcontrol", "cmd:load", $"album_id:{id}" });
            var url = string.Concat(BaseUrl, "jsonrpc.js");

            string json = JsonSerializer.Serialize(request);
            var response = client.PostAsync(url, new StringContent(json)).Result;
            var res = response.Content.ReadAsStringAsync().Result;

            JsonDocument jdoc = JsonDocument.Parse(res);
            var r = jdoc.RootElement.GetProperty("result");
            int cnt = r.GetProperty("count").GetInt32();
            return cnt;
        }

#region *************** Impl of IMediaRepository

        public async Task LoadAllAsync(object PersitenceContext) {
            var rads = await GetRadiosAsync();
            foreach (var rad in rads) {
                radioList.Add(rad);
            }

            var albs = await GetAlbumsAsync();
            foreach (var alb in albs) {
                albumList.Add(alb);
            }
        }

        public ObservableCollection<MediaCategory> GetCategories() {
            return mediaCategories;
        }

        public ObservableCollection<IMedia> GetMediaRepository(string categoryId) {
            if (mediaRepositories.TryGetValue(categoryId, out ObservableCollection<IMedia>? value)) {
                return value;
            } else {
                return [];
            }
        }

        // This should go private - is only needed because assets are not listable by directory in Avolon impl....
        public Task LoadReposAsync(string context, Stream streamReader) {
            throw new NotImplementedException();
        }

#endregion
        public void PlayCd(IMedia cd) {
            PlayAlbum(CurrentActive.Id, (cd as LmsObject).Id);
        }

        public void PlayRadio(IMedia radio) {
            PlayRadio(CurrentActive.Id, (radio as LmsObject).Id);
        }

        private IPlayerProxy? CurrentActive;

        public void SetActiveClient(IPlayerProxy? value) {
            CurrentActive = value;
        }

        public async Task LoadAllAsync() {
            var players = await GetPlayersAsync();
            foreach (var player in players) {
                KnownPlayer.Add(player);
                PlayerFound?.Invoke(this, player);
            }
        }

        public Task TryConnectAsync(IPlayerProxy ccw) {
            CurrentActive = ccw;
            return Task.CompletedTask;
        }
    }


}
