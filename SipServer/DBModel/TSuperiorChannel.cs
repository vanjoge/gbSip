using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class TSuperiorChannel
    {
        /// <summary>
        /// 上级ID
        /// </summary>
        public string SuperiorId { get; set; }
        /// <summary>
        /// 自定义通道ID
        /// </summary>
        public string CustomChannelId { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// CatalogID
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 启用
        /// </summary>
        public bool Enable { get; set; }
    }
}
