using KeiseiZaisenSharp.RawEntities.IkisakiInfos;
using KeiseiZaisenSharp.RawEntities.StationInfos;
using KeiseiZaisenSharp.RawEntities.StopInfos;
using KeiseiZaisenSharp.RawEntities.SyasyuInfos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp
{
    public class KeiseiZaisenConfigurationSources
    {
        public IkisakiEntry[] Ikisakis
        {
            get;
        }

        public StationEntry[] Stations
        {
            get;
        }

        public StopEntry[] Stops
        {
            get;
        }

        public SyasyuEntry[] Syasyus
        {
            get;
        }


        public KeiseiZaisenConfigurationSources(IkisakiEntry[] ikisakiEntries, StationEntry[] stationEntries, StopEntry[] stopEntries, SyasyuEntry[] syasyuEntries)
        {
            this.Ikisakis = ikisakiEntries;
            this.Stations = stationEntries;
            this.Stops = stopEntries;
            this.Syasyus = syasyuEntries;
        }

        /// <summary>
        /// <see cref="StopEntry"/> から隣の駅を取得します。
        /// </summary>
        /// <param name="stopEntry"></param>
        /// <param name="direction">進行方向 (0 = 上り, 1 = 下り)</param>
        /// <returns>見つからない場合、向こうな場合は <see cref="null"/> が戻ります</returns>
        public StopEntry? GetNextStop(StopEntry stopEntry, int direction = 0)
        {
            var currentNo = Array.IndexOf(this.Stops, stopEntry);
            if (currentNo < 0)
                return null;

            if (direction == 1 && this.Stops.Length > currentNo + 1)
                return this.Stops[currentNo + 1];
            if (direction == 0 && currentNo > 0)
                return this.Stops[currentNo - 1];

            return null;
        }
    }
}
