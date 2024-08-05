using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class TJTinfo
    {
        /// <summary>
        /// CatalogID
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 1078SIM卡号
        /// </summary>
        public string JTsim { get; set; }
        /// <summary>
        /// 1078通道号
        /// </summary>
        public int JTchannel { get; set; }
        /// <summary>
        /// 是否2019版本
        /// </summary>
        public ulong Is2019 { get; set; }
    }
}
