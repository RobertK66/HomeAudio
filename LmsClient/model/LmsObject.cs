using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsClient.model {
    public class LmsObject {
        public String Id { get; set; }
        public String Name { get; set; }

        public LmsObject(String id, String name) {
            Id = id;
            Name = name;
        }
    }
}
