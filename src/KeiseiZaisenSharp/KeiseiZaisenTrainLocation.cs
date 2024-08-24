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
        private TrafficRecord _trafficRecord;

        /// <summary>
        /// 現在の位置を表します。
        /// 例: [走行中, 下り] 八千代台 => 実籾
        /// </summary>
        public string Description
        {
            get
            {
                switch (this.Status)
                {
                    case KeiseiZaisenTrainStatus.StoppingOrPassing:
                        return $"[停車・通過中] {this.CurrentOrNextStation} ({this.Track} 番線)";
                    case KeiseiZaisenTrainStatus.RunningToDown:
                        return $"[走行中, 下り] {this.PrevStation ?? "(不明)"} => {this.CurrentOrNextStation}";
                    case KeiseiZaisenTrainStatus.RunningToUp:
                        return $"[走行中, 上り] {this.PrevStation ?? "(不明)"} => {this.CurrentOrNextStation}";
                }

                return "[不明]";
            }
        }

        /// <summary>
        /// 現在の状態を取得します。
        /// </summary>
        public KeiseiZaisenTrainStatus Status
        {
            get
            {
                if (String.IsNullOrEmpty(this.RawSource.Id))
                    return KeiseiZaisenTrainStatus.Unknown;

                var c = this.RawSource.Id[0];
                if (c == 'E')
                    return KeiseiZaisenTrainStatus.StoppingOrPassing;
                if (c == 'D')
                    return KeiseiZaisenTrainStatus.RunningToDown;
                if (c == 'U')
                    return KeiseiZaisenTrainStatus.RunningToUp;

                return KeiseiZaisenTrainStatus.Unknown;
            }
        }

        /// <summary>
        /// 駅での通過中・停車中の場合、番線を取得します。
        /// </summary>
        public int Track
        {
            get
            {
                var bs = 0;
                if (Int32.TryParse(this._trafficRecord.Bs, out bs))
                    return bs;
                return -1;
            }
        }

        /// <summary>
        /// 現在の停車・通過駅、または次の停車駅の <see cref="StopEntry"/> を取得します。
        /// </summary>
        [JsonIgnore]
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

        /// <summary>
        /// 現在の停車・通過駅、または次の停車駅の名前を取得します。
        /// </summary>
        public string? CurrentOrNextStation
        {
            get => this.CurrentOrNextStationInfo?.Name;
        }

        /// <summary>
        /// 駅間走行中の場合、直前の停車駅の名前を取得します。
        /// </summary>
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

        /// <summary>
        /// サーバから取得した生データを取得します。
        /// </summary>
        [JsonIgnore]
        public TrafficSection RawSource
        {
            get;
            private set;
        }

        /// <summary>
        /// <see cref="KeiseiZaisenTrainLocation"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="configurationSources"></param>
        /// <param name="rawSource"></param>
        /// <param name="rawRecord"></param>
        public KeiseiZaisenTrainLocation(KeiseiZaisenConfigurationSources configurationSources, TrafficSection rawSource, TrafficRecord rawRecord)
        {
            this._configurationSources = configurationSources;
            this._trafficRecord = rawRecord;

            this.RawSource = rawSource;
        }
    }
}
