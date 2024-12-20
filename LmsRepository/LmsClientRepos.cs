﻿using AudioCollectionApi.api;
using AudioCollectionApi.model;
using LmsRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LmsRepositiory {
    public class LmsClientRepos : IMediaRepository, IPlayerRepository {
        private static int instanceCount = 0;
        private int instanceid = 0;
        private ILogger? _log;
        private HttpClient _client = new HttpClient();

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

        public LmsClientRepos(ILogger<LmsClientRepos>? log = null) {
            mediaRepositories.Add("Radios", radioList);
            mediaRepositories.Add("CDs", albumList);
            _log = log;
            _log?.LogDebug($"Constructor [{++instanceCount}] finished.");
            instanceid = instanceCount;
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

            JsonElement r = await GetLmsResponseAsync(request);

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

        private async Task<JsonElement> GetLmsResponseAsync(LmsJsonRequest request) {
            JsonElement r = new JsonElement();
            var req = request.GetStringContent();

            _log?.LogTrace($"Send[{instanceid}]: '{req.ReadAsStringAsync().Result}' to {BaseUrl}");

            try {
                var response = await _client.PostAsync(BaseUrl, req);
                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    var res = response.Content.ReadAsStringAsync().Result;
                    _log?.LogTrace($"Recv[{instanceid}]: '{res}'");
                    JsonDocument jdoc = JsonDocument.Parse(res);
                    r = jdoc.RootElement.GetProperty("result");
                } else {
                    _log?.LogError($"Status code on Lms Request: '{response.StatusCode}'");
                }
            } catch (Exception ex) {
                _log?.LogError($"Error on Lms Request: '{ex.Message}'");
            }
            
            return r;
        }

        public void PlayRadio(string playerid, string? id) {
            //Request: "6e:ef:54:e9:02:b0 favorites playlist play item_id:1.1<LF>"
            var request = new LmsJsonRequest(playerid, new object[] { "favorites", "playlist", "play", $"item_id:{id}" });

            _ = GetLmsResponseAsync(request);

            _ = GetPlayerStatusAsync(playerid);

            // no usefull content in result....
            //var res = response.Content.ReadAsStringAsync().Result;
            //JsonDocument jdoc = JsonDocument.Parse(res);
            //var r = jdoc.RootElement.GetProperty("result");


        }

        public async Task<int> PlayAlbum(string playerid, string? id) {
            //Request: "a5:41:d2:cd:cd:05 playlistcontrol cmd:load album_id:22<LF>"

            // Dieses Load ist nachdem der JBL Chromecast LS abgeschaltet wurde zu wenig - es startet das Play nicht - ON/OFF !? mit radio scheit es zu funkrtionieren....????
            var request = new LmsJsonRequest(playerid, new object[] { "playlistcontrol", "cmd:load", $"album_id:{id}" });
            //var response = _client.PostAsync(BaseUrl, request.GetStringContent()).Result;

            var r = await GetLmsResponseAsync(request); 
            int cnt = r.GetProperty("count").GetInt32();

            _ = GetPlayerStatusAsync(playerid);

            return cnt;
        }

        public void PlayCd(string playerId, IMedia cd) {
            _ = PlayAlbum(playerId, (cd as LmsObject).Id);
        }

        public void PlayRadio(string playerId, IMedia radio) {
            PlayRadio(playerId, (radio as LmsObject).Id);
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
        bool loadAllTriggered = false;
        public async Task LoadReposAsync(string context, Stream streamReader) {
            if (!loadAllTriggered) {
                loadAllTriggered = true;
                await LoadAllAsync(context);
            }
        }

        #endregion
      
        //private IPlayerProxy? CurrentActive;

        //public void SetActiveClient(IPlayerProxy? value) {
        //    CurrentActive = value;
        //}

        public async Task LoadAllAsync() {
            var players = await GetPlayersAsync();
            foreach (var player in players) {
                KnownPlayer.Add(player);
                PlayerFound?.Invoke(this, player);
            }
        }


        private async Task HandleTimerAsync(IPlayerProxy ccw) {
            if (ccw != null) {
                if (ccw.IsConnected) {
                    await GetPlayerStatusAsync(ccw.Id);
                }
            }
        }


        private Dictionary<string, System.Timers.Timer> Timers = new Dictionary<string, System.Timers.Timer>();

        public void Disconnect(IPlayerProxy p) {
            System.Timers.Timer? timer;
            if (Timers.TryGetValue(p.Id, out timer)) {
                timer.Stop();
            }
        }

        public async Task TryConnectAsync(IPlayerProxy ccw) {
            //CurrentActive = ccw;
            await GetPlayerStatusAsync(ccw.Id);

            System.Timers.Timer? timer;
            if (!Timers.TryGetValue(ccw.Id, out timer)) {
                timer = new(interval: 3000);
                Timers.Add(ccw.Id, timer);
                timer.Elapsed += async (sender, e) => await HandleTimerAsync(ccw).ConfigureAwait(true);
            } 
            timer.Start();
            
            ccw.IsConnected = true;
            //// var request = new LmsJsonRequest(ccw.Id, new object[] { "status", "-", "5", "subscribe:10" });  //n.i if and how a long http would work here ....????
            //// TODO: figure out how to subscribe and/or make polling in background here.....
            //var request = new LmsJsonRequest(ccw.Id, new object[] { "status", "-", "5", "tags:" });
            //var response = await _client.PostAsync(BaseUrl, request.GetStringContent());

            //var res = response.Content.ReadAsStringAsync().Result;
            //JsonDocument jdoc = JsonDocument.Parse(res);
            //var r = jdoc.RootElement.GetProperty("result");

            //try {
            //    ccw.Status = r.GetProperty("mode").GetString() ?? "";
            //    var playlist = r.GetProperty("playlist_loop").EnumerateArray();
            //    var first = playlist.FirstOrDefault().GetProperty("title").GetString();
            //    ccw.MediaStatus = first;
            //    ccw.IsConnected = true;
            //} catch (Exception) {

            //}
        }

    

        internal void VolumeUp(string playerId) {
            //Request: "04:20:00:12:23:45 mixer volume ?<LF>"
            //Response: "04:20:00:12:23:45 mixer volume 98<LF>"
            //
            //Request: "04:20:00:12:23:45 mixer volume 25<LF>"
            //Response: "04:20:00:12:23:45 mixer volume 25<LF>"
            //    
            //Request: "04:20:00:12:23:45 mixer volume +10<LF>"
            //Response: "04:20:00:12:23:45 mixer volume +10<LF>"

            var request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", "+3" });
            _ = GetLmsResponseAsync(request);
            _ = GetPlayerStatusAsync(playerId);


            //var response = _client.PostAsync(BaseUrl, request.GetStringContent()).Result;

            //var res = response.Content.ReadAsStringAsync().Result;
            //JsonDocument jdoc = JsonDocument.Parse(res);
            //var r =  jdoc.RootElement.GetProperty("result");

            //int p = 50;
            //string value = r.GetProperty("_volume").ToString();
            //if (Int32.TryParse(value, out p)) {
            //    p += 3;
            //}
            //if (p > 100) {
            //    p = 100;
            //}
            //request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", p.ToString() });
            //_ = GetLmsResponseAsync(request);
            ////response = _client.PostAsync(BaseUrl, request.GetStringContent()).Result;
            ////res = response.Content.ReadAsStringAsync().Result;

            //return p;
        }




        internal void VolumeDown(string playerId) {
            var request = new LmsJsonRequest(playerId, new object[] { "mixer", "volume", "-3" });
            _ = GetLmsResponseAsync(request);
            _ = GetPlayerStatusAsync(playerId);
        }

        public async Task<String> GetPlayerStatusAsync(string playerId) {

            var request = new LmsJsonRequest(playerId, new object[] { "status", "-", "50" });

            var r = await GetLmsResponseAsync(request);
            //var response = await _client.PostAsync(BaseUrl, request.GetStringContent());
            //var res = response.Content.ReadAsStringAsync().Result;
            //JsonDocument jdoc = JsonDocument.Parse(res);
            //var r = jdoc.RootElement.GetProperty("result");

            var player = KnownPlayer.Where(p => p.Id == playerId).FirstOrDefault();
            if (player != null) {
                try {
                    JsonElement val;
                    if (r.TryGetProperty("mixer volume", out val)) {
                        player.Volume = val.GetInt32();
                    }
                    if (r.TryGetProperty("mode", out val)) {
                        player.Status = val.ToString();
                    }
                    // If the LMS has a connection to the player we model this as IsOn -> IPlayerProxy.IsConnected shows that we (the controller) have a connection moniotoring the player
                    //  FormatException LMS this means we do have polling on and should get the changed status once LMS reestablishes connection to the real player...
                    if (r.TryGetProperty("player_connected", out val)) {
                        if (val.GetInt32() == 0) {
                            player.IsOn = false;
                        } else {
                            player.IsOn = true;
                        }
                    }

                    if (player.Status == "stop") {
                        player.MediaStatus = "";
                    } else {

                        if (r.TryGetProperty("playlist_loop", out val)) {
                            var playlist = val.EnumerateArray();
                            if (playlist.FirstOrDefault().TryGetProperty("title", out val)) {
                                var first = val.GetString();
                                player.MediaStatus = first;
                            }
                        }
                    }
                } catch (Exception ex) {
                    _log?.LogError(ex, "Fehler beim deserialize");
                }
            }

            return JsonSerializer.Serialize(r, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task StopPlayAsync(string playerId) {
            //Request: "04:20:00:12:23:45 stop<LF>"
            //Response: "04:20:00:12:23:45 stop<LF>"
            var request = new LmsJsonRequest(playerId, new object[] { "stop" });
            var r = await GetLmsResponseAsync(request);
            await GetPlayerStatusAsync(playerId);
        }

        public async Task PlayAsync(string playerId) {
            //Request: "04:20:00:12:23:45 stop<LF>"
            //Response: "04:20:00:12:23:45 stop<LF>"
            var request = new LmsJsonRequest(playerId, new object[] { "play" });
            await GetLmsResponseAsync(request);
            await GetPlayerStatusAsync(playerId);
        }

    }

}
