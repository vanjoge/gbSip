using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    /// <summary>
    /// 分组绑定通道
    /// </summary>
    public partial class TGroupBind
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 绑定设备ID
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 绑定通道ID(0代表所有)
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 自定义通道ID(上报用)
        /// </summary>
        public string CustomChannelId { get; set; }
    }
}
