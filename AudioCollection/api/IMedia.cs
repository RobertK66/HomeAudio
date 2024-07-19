using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCollectionApi.api {
    public interface IMedia {
        bool IsCollection { get; }
        IList<IMedia> Content { get; }

        string Name { get; }
        string? ContentUrl {  get; }
    }
}
