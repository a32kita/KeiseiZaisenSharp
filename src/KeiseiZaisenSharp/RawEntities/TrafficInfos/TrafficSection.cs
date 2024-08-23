using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp.RawEntities.TrafficInfos
{
    public class TrafficSection
    {
        public string Id { get; set; }
        public List<TrafficRecord> Tr { get; set; }
    }
}
