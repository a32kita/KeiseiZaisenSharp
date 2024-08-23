using System;
using System.Collections.Generic;
using System.Text;

namespace KeiseiZaisenSharp
{
    public enum KeiseiZaisenTrainStatus
    {
        /// <summary>
        /// 不明
        /// </summary>
        Unknown       = 0b0000,

        /// <summary>
        /// 駅に停車中
        /// </summary>
        Stopping      = 0b0001,

        /// <summary>
        /// 走行中
        /// </summary>
        Running       = 0b1000,

        /// <summary>
        /// 走行中 (下り線)
        /// </summary>
        RunningToDown = 0b1010,

        /// <summary>
        /// 走行中 (上り線)
        /// </summary>
        RunningToUp   = 0b1100,
    }
}
