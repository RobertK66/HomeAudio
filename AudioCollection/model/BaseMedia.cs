﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCollectionApi.model {

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(BaseMedia))]
    [JsonDerivedType(typeof(Cd), typeDiscriminator: "cd")]
    [JsonDerivedType(typeof(NamedUrl), typeDiscriminator: "radio")]
    public class BaseMedia {
        public string Name { get; set; }
    }
}
