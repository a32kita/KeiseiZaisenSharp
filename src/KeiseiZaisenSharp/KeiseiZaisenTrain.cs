using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using KeiseiZaisenSharp.RawEntities.TrafficInfos;

namespace KeiseiZaisenSharp
{
    public class KeiseiZaisenTrain
    {
        private KeiseiZaisenConfigurationSources _configurationSources;

        /// <summary>
        /// 列車番号
        /// </summary>
        public string? TrainNumber
        {
            get => this.RawSource.No;
        }

        /// <summary>
        /// 行き先
        /// </summary>
        public string? Destination
        {
            get => this._configurationSources.Ikisakis.SingleOrDefault(item => item.Code == this.RawSource.Ik)?.Name;
        }

        /// <summary>
        /// 種別
        /// </summary>
        public string? TrainType
        {
            get => this._configurationSources?.Syasyus.SingleOrDefault(item => item.Code == this.RawSource.Sy)?.Name;
        }

        /// <summary>
        /// 現在の位置
        /// </summary>
        public KeiseiZaisenTrainLocation Location
        {
            get;
        }

        /// <summary>
        /// サーバから取得した生データ
        /// </summary>
        public TrafficRecord RawSource
        {
            get;
        }


        public KeiseiZaisenTrain(KeiseiZaisenConfigurationSources configurationSources, TrafficSection trafficSection, int numberInSection)
        {
            this._configurationSources = configurationSources;

            this.RawSource = trafficSection.Tr[numberInSection];
            this.Location = new KeiseiZaisenTrainLocation(configurationSources, trafficSection);
        }
    }
}
