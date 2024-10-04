using AudioCollectionApi.api;
using AudioCollectionApi.model;
using LmsRepository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LmsRepositiory {
    public class LmsClientRepos : IMediaRepository, IPlayerRepository {
        HttpClient client = new HttpClient();

        private ObservableCollection<MediaCategory> mediaCategories = new ObservableCollection<MediaCategory>() {
            new MediaCategory("Radios"){ Name="Radios" },
            new MediaCategory("CDs"){ Name="CDs" }
        };

        private ObservableCollection<IMedia> radioList = new ObservableCollection<IMedia>();
        private ObservableCollection<IMedia> albumList = new ObservableCollection<IMedia>();

        private Dictionary<String, ObservableCollection<IMedia>> mediaRepositories = new Dictionary<String, ObservableCollection<IMedia>>();

        public event EventHandler<IPlayerProxy>? PlayerFound;

        public string BaseUrl { get; set; } = "http://192.168.177.65:9000/jsonrpc.js";

        private ObservableCollection<IPlayerProxy> _knownPlayer = new ObservableCollection<IPlayerProxy>();
        public ObservableCollection<IPlayerProxy> KnownPlayer { get { return _knownPlayer; } }

        public LmsClientRepos() {
            mediaRepositories.Add("Radios", radioList);
            mediaRepositories.Add("CDs", albumList);
        }

        public async Task<IEnumerable<LmsPlayer>> GetPlayersAsync() {
            List<LmsPlayer> players = new List<LmsPlayer>();
            IEnumerable<LmsObject> list = await GetLmsObjectsAsync<LmsObject>(new LmsJsonRequest(string.Empty, new object[] { "players", 0, 10 }), "players_loop", "playerid", "name");
            foreach (var p in list) {
                players.Add(new LmsPlayer(p.Id, p.Name, this));
            }
            return players;
        }

        public async Task<IEnumerable<LmsObject>> GetAlbumsAsync() {
            return await GetLmsObjectsAsync<LmsObject>(new LmsJsonRequest(string.Empty, new object[] { "albums", 0, 300, "sort:album" }), "albums_loop", "id", "album");
        }

        public async Task<IEnumerable<LmsObject>> GetRadiosAsync() {
            return await GetLmsObjectsAsync<LmsObject>(new LmsJsonRequest(string.Empty, new object[] { "favorites", "items", 0, 50 }), "loop_loop", "id", "name");
        }

        private async Task<IEnumerable<T>> GetLmsObjectsAsync<T>(LmsJsonRequest request, string loopname, string idprop, string nameprop) {
            List<T> retVal = new List<T>();
            bool isCollection = nameprop.Equals("album");

            var response = await client.PostAsync(BaseUrl, request.GetStringContent());
            var res = response.Content.ReadAsStringAsync().Result;

            JsonDocument jdoc = JsonDocument.Parse(res);

            var r = jdoc.RootElement.GetProperty("result");
            var p = r.GetProperty(loopname);
            foreach (var x in p.EnumerateArray()) {
                try {
                    var val = (T?)Activator.CreateInstance(typeof(T), new object[] { x.GetProperty(idprop).ToString(), x.GetProperty(nameprop).ToString(), isCollection });
                    if (val != null) {
                        retVal.Add(val);
                    }
                    //retVal.Add(new LmsObject(x.GetProperty(idprop).ToString(), x.GetProperty(nameprop).ToString(), isCollection));
                } catch (Exception) {
                    // TODO: logging //error out 
                }
            }
            return retVal;
        }

        public void PlayRadio(string playerid, string? id) {
            //Request: "6e:ef:54:e9:02:b0 favorites playlist play item_id:1.1<LF>"
            var request = new LmsJsonRequest(playerid, new object[] { "favorites", "playlist", "play", $"item_id:{id}" });
            var response = client.PostAsync(BaseUrl, request.GetStringContent()).Result;

            // no usefull content in result....
            //var res = response.Content.ReadAsStringAsync().Result;
            //JsonDocument jdoc = JsonDocument.Parse(res);
            //var r = jdoc.RootElement.GetProperty("result");


        }

        public int PlayAlbum(string playerid, string? id) {
            //Request: "a5:41:d2:cd:cd:05 playlistcontrol cmd:load album_id:22<LF>"
            var request = new LmsJsonRequest(playerid, new object[] { "playlistcontrol", "cmd:load", $"album_id:{id}" });
            var response = client.PostAsync(BaseUrl, request.GetStringContent()).Result;

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


        public async Task TryConnectAsync(IPlayerProxy ccw) {
            CurrentActive = ccw;
            var request = new LmsJsonRequest(ccw.Id, new object[] { "status", "-", "5", "tags:" });
            var response = await client.PostAsync(BaseUrl, request.GetStringContent());

            var res = response.Content.ReadAsStringAsync().Result;
            JsonDocument jdoc = JsonDocument.Parse(res);
            var r = jdoc.RootElement.GetProperty("result");

            try {
                ccw.Status = r.GetProperty("mode").GetString() ?? "";
                var playlist = r.GetProperty("playlist_loop").EnumerateArray();
                var first = playlist.FirstOrDefault().GetProperty("title").GetString();
                ccw.MediaStatus = first;
                ccw.IsConnected = true;
            } catch (Exception) {

            }
        }

        internal int VolumeUp(string playerId) {
            //Request: "04:20:00:12:23:45 mixer volume ?<LF>"
            //Response: "04:20:00:12:23:45 mixer volume 98<LF>"
            //
            //Request: "04:20:00:12:23:45 mixer volume 25<LF>"
            //Response: "04:20:00:12:23:45 mixer volume 25<LF>"
            //    
            //Request: "04:20:00:12:23:45 mixer volume +10<LF>"
            //Response: "04:20:00:12:23:45 mixer volume +10<LF>"

            var request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", "?" });
            var response = client.PostAsync(BaseUrl, request.GetStringContent()).Result;

            var res = response.Content.ReadAsStringAsync().Result;
            JsonDocument jdoc = JsonDocument.Parse(res);
            var r = jdoc.RootElement.GetProperty("result");

            int p = 50;
            string value = r.GetProperty("_volume").ToString();
            if (Int32.TryParse(value, out p)) {
                p += 3;
            }
            if (p > 100) {
                p = 100;
            }
            request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", p.ToString() });
            response = client.PostAsync(BaseUrl, request.GetStringContent()).Result;
            res = response.Content.ReadAsStringAsync().Result;

            return p;
        }




        internal int VolumeDown(string playerId) {
            var request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", "?" });
            var response = client.PostAsync(BaseUrl, request.GetStringContent()).Result;

            var res = response.Content.ReadAsStringAsync().Result;
            JsonDocument jdoc = JsonDocument.Parse(res);
            var r = jdoc.RootElement.GetProperty("result");

            int p = 50;
            string value = r.GetProperty("_volume").ToString();
            if (Int32.TryParse(value, out p)) {
                p -= 3;
            }
            if (p < 0) {
                p = 0;
            }
            request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", p.ToString() });
            response = client.PostAsync(BaseUrl, request.GetStringContent()).Result;
            res = response.Content.ReadAsStringAsync().Result;

            return p;
        }

    }

}
