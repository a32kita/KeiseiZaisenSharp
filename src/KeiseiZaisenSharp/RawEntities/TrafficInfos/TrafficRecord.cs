using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp.RawEntities.TrafficInfos
{
    public class TrafficRecord
    {
        /// <summary>
        /// 列車番号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 番線？
        /// </summary>
        public string Bs { get; set; }

        /// <summary>
        /// 種別
        /// </summary>
        public string Sy { get; set; }

        /// <summary>
        /// 行き先
        /// </summary>
        public string Ik { get; set; }

        /// <summary>
        /// 遅延
        /// </summary>
        public string Dl { get; set; }

        /// <summary>
        /// 方向 ("1" = 上野方面 / "2" = 成田方法面)
        /// </summary>
        public string Hk { get; set; }

        /// <summary>
        /// 編成車両数
        /// </summary>
        public string Sr { get; set; }
    }
}
