using KeiseiZaisenSharp.RawEntities.StopInfos;
using KeiseiZaisenSharp.RawEntities.TrafficInfos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace KeiseiZaisenSharp
{
    public class KeiseiZaisenTrainLocation
    {
        private KeiseiZaisenConfigurationSources _configurationSources;

        public string Description
        {
            get
            {
                //foreach (var st in this._configurationSources.Stops)
                //{
                //    var codeInt = 0;
                //    if (!Int32.TryParse(st.Code, out codeInt))
                //        continue;

                //    if (this.RawSource.Id == $"E{codeInt.ToString("000")}")
                //        return $"[停車中] {st.Name}";

                //    if (this.RawSource.Id == $"D{codeInt.ToString("000")}")
                //    {
                //        var prevStp = this._configurationSources.GetNextStop(st, 0);
                //        return $"[走行中, 下り] {prevStp?.Name} => {st.Name}";
                //    }

                //    if (this.RawSource.Id == $"U{codeInt.ToString("000")}")
                //    {
                //        var prevStp = this._configurationSources.GetNextStop(st, 1);
                //        return $"[走行中, 上り] {prevStp?.Name} => {st.Name}";
                //    }
                //}

                //return "[不明]";

                switch (this.Status)
                {
                    case KeiseiZaisenTrainStatus.Stopping:
                        return $"[停車中] {this.CurrentOrNextStation}";
                    case KeiseiZaisenTrainStatus.RunningToDown:
                        return $"[走行中, 下り] {this.PrevStation} => {this.CurrentOrNextStation}";
                    case KeiseiZaisenTrainStatus.RunningToUp:
                        return $"[走行中, 上り] {this.PrevStation} => {this.CurrentOrNextStation}";
                }

                return "[不明]";
            }
        }

        public KeiseiZaisenTrainStatus Status
        {
            get
            {
                if (String.IsNullOrEmpty(this.RawSource.Id))
                    return KeiseiZaisenTrainStatus.Unknown;

                var c = this.RawSource.Id[0];
                if (c == 'E')
                    return KeiseiZaisenTrainStatus.Stopping;
                if (c == 'D')
                    return KeiseiZaisenTrainStatus.RunningToDown;
                if (c == 'U')
                    return KeiseiZaisenTrainStatus.RunningToUp;

                return KeiseiZaisenTrainStatus.Unknown;
            }
        }

        public StopEntry? CurrentOrNextStationInfo
        {
            get
            {
                foreach (var st in this._configurationSources.Stops)
                {
                    var codeInt = 0;
                    if (!Int32.TryParse(st.Code, out codeInt))
                        continue;
                    if (this.RawSource.Id.Substring(1) != codeInt.ToString("000"))
                        continue;
                    
                    return st;
                }

                return null;
            }
        }

        public string? CurrentOrNextStation
        {
            get => this.CurrentOrNextStationInfo?.Name;
        }

        public string? PrevStation
        {
            get
            {
                var cStp = this.CurrentOrNextStationInfo;
                if (cStp == null)
                    return null;
                var pStp = cStp;

                switch (this.Status)
                {
                    case KeiseiZaisenTrainStatus.RunningToDown:
                        pStp = this._configurationSources.GetNextStop(cStp, 0);
                        break;
                    case KeiseiZaisenTrainStatus.RunningToUp:
                        pStp = this._configurationSources.GetNextStop(cStp, 1);
                        break;
                }

                return pStp?.Name;
            }
        }

        [JsonIgnore]
        public TrafficSection RawSource
        {
            get;
            private set;
        }

        public KeiseiZaisenTrainLocation(KeiseiZaisenConfigurationSources configurationSources, TrafficSection rawSource)
        {
            this._configurationSources = configurationSources;

            this.RawSource = rawSource;
        }
    }
}
