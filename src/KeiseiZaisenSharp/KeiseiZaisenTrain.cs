using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using KeiseiZaisenSharp.RawEntities.TrafficInfos;
using System.Text.Json.Serialization;

namespace KeiseiZaisenSharp
{
    public class KeiseiZaisenTrain
    {
        private KeiseiZaisenConfigurationSources _configurationSources;

        /// <summary>
        /// 列車番号を取得します。
        /// </summary>
        public string? TrainNumber
        {
            get => this.RawSource.No;
        }

        /// <summary>
        /// 行き先を取得します。
        /// </summary>
        public string? Destination
        {
            get => this._configurationSources.Ikisakis.SingleOrDefault(item => item.Code == this.RawSource.Ik)?.Name;
        }

        /// <summary>
        /// 種別を取得します。
        /// </summary>
        public string? TrainType
        {
            get => this._configurationSources?.Syasyus.SingleOrDefault(item => item.Code == this.RawSource.Sy)?.Name;
        }

        /// <summary>
        /// 現在の列車の位置を取得します。
        /// </summary>
        public KeiseiZaisenTrainLocation Location
        {
            get;
        }

        /// <summary>
        /// 遅延 (min) を取得します。
        /// </summary>
        public int? Delay
        {
            get
            {
                var d = 0;
                if (Int32.TryParse(this.RawSource.Dl, out d))
                    return d;
                return null;
            }
        }

        /// <summary>
        /// サーバから取得した生データを取得します。
        /// </summary>
        [JsonIgnore]
        public TrafficRecord RawSource
        {
            get;
        }


        /// <summary>
        /// <see cref="KeiseiZaisenTrain"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="configurationSources"></param>
        /// <param name="trafficSection"></param>
        /// <param name="numberInSection"></param>
        public KeiseiZaisenTrain(KeiseiZaisenConfigurationSources configurationSources, TrafficSection trafficSection, int numberInSection)
        {
            this._configurationSources = configurationSources;

            this.RawSource = trafficSection.Tr[numberInSection];
            this.Location = new KeiseiZaisenTrainLocation(configurationSources, trafficSection, this.RawSource);
        }
    }
}
