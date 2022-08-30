using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class TEvent
    {
        public ulong RowId { get; set; }
        /// <summary>
        /// CatalogID
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 状态改变事件 0-ON:上线,1-OFF:离线,2-VLOST:视频丢失,3-DEFECT:故障,4-ADD:增加,5-DEL:删除,6-UPDATE:更新
        /// </summary>
        public int Event { get; set; }
        /// <summary>
        /// 事件时间
        /// </summary>
        public DateTime EventTime { get; set; }
    }
}
