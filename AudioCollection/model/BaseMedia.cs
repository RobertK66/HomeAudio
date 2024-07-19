using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCollectionApi {

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(BaseMedia))]
    [JsonDerivedType(typeof(AudioCollectionApi.Cd), typeDiscriminator: "cd")]
    [JsonDerivedType(typeof(AudioCollectionApi.NamedUrl), typeDiscriminator: "radio")]
    public class BaseMedia {
        public required string Name { get; set; }

        [SetsRequiredMembers]
        public BaseMedia() { Name = "empty"; }
    }
}
