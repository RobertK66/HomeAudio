using LmsClient.cmd;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static LmsClient.cmd.ListCmd;

namespace LmsClient.model {
    public class LmsClientRepos {
        HttpClient client = new HttpClient();

        public string BaseUrl { get; internal set; }

        public IEnumerable<LmsObject> GetPlayers() {
            return GetLmsObjects(new LmsJsonRequest(string.Empty, new object[] { "players", 0, 10 }), "players_loop", "playerid", "name");
        }

        public IEnumerable<LmsObject> GetAlbums() {
            return GetLmsObjects(new LmsJsonRequest(string.Empty, new object[] { "albums", 0, 300, "sort:album" }), "albums_loop", "id", "album");
        }

        public IEnumerable<LmsObject> GetRadios() {
            return GetLmsObjects(new LmsJsonRequest(string.Empty, new object[] { "favorites", "items", 0, 50 }), "loop_loop", "id", "name");
        }

        private IEnumerable<LmsObject> GetLmsObjects(LmsJsonRequest request, string loopname, string idprop, string nameprop) {
            List<LmsObject> retVal = new List<LmsObject>();
            var url = string.Concat(BaseUrl, "jsonrpc.js");

            string json = JsonSerializer.Serialize(request);
            var response = client.PostAsync(url, new StringContent(json)).Result;
            var res =  response.Content.ReadAsStringAsync().Result;

            JsonDocument jdoc = JsonDocument.Parse(res);

            var r = jdoc.RootElement.GetProperty("result");
            var p = r.GetProperty(loopname);
            foreach(var x in  p.EnumerateArray()) {
                try {
                    retVal.Add(new LmsObject(x.GetProperty(idprop).ToString(), x.GetProperty(nameprop).ToString()));
                } catch (Exception) {
                    // TODO: logging //error out 
                }
            }
            return retVal;
        }


    }
}
