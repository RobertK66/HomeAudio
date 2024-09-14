﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LmsClient.model {
    public class LmsJsonRequest {
        private static int gid = 1;
        private readonly List<object> _parameters;

        public LmsJsonRequest(string playerIdentifier, object[] args) {
            _parameters = [playerIdentifier, args];
        }

        [JsonPropertyName("method")]
        public string Method { get { return "slim.request"; } }

        [JsonPropertyName("id")]
        public int Id { get { return gid++; } }

        [JsonPropertyName("params")]
        public IEnumerable<object> Parameters { get { return _parameters; } }
    }
}
