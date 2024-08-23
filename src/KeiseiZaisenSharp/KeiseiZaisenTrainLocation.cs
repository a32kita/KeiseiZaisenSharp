using KeiseiZaisenSharp.RawEntities.StopInfos;
using KeiseiZaisenSharp.RawEntities.TrafficInfos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp
{
    public class KeiseiZaisenTrainLocation
    {
        private KeiseiZaisenConfigurationSources _configurationSources;

        public string Description
        {
            get
            {
                foreach (var st in this._configurationSources.Stops)
                {
                    var codeInt = 0;
                    if (!Int32.TryParse(st.Code, out codeInt))
                        continue;

                    if (this.RawSource.Id == $"E{codeInt.ToString("000")}")
                        return $"[停車中] {st.Name}";

                    if (this.RawSource.Id == $"D{codeInt.ToString("000")}")
                    {
                        var prevStp = this._configurationSources.GetNextStop(st, 0);
                        return $"[走行中, 下り] {prevStp?.Name} => {st.Name}";
                    }

                    if (this.RawSource.Id == $"U{codeInt.ToString("000")}")
                    {
                        var prevStp = this._configurationSources.GetNextStop(st, 1);
                        return $"[走行中, 上り] {prevStp?.Name} => {st.Name}";
                    }
                }

                return "[不明]";
            }
        }

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
