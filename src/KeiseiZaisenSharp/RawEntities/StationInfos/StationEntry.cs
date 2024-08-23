using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp.RawEntities.StationInfos
{
    public class StationEntry
    {
        //public string Type { get; set; }
        //public Geometry Geometry { get; set; }
        //public StationProperties Properties { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public Uri Link { get; set; }
    }
}
