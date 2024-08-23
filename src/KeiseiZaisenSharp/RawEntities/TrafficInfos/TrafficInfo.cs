using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp.RawEntities.TrafficInfos
{
    public class TrafficInfo
    {
        public List<UpdateInfo> UP { get; set; }
        public List<TrafficSection> TS { get; set; }
        public List<TrafficSection> EK { get; set; }
    }
}
