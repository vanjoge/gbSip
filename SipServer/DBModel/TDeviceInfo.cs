using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class TDeviceInfo
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string Did { get; set; }
        /// <summary>
        /// 目标设备/区域/系统的名称(可选)
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 设备生产商(可选)
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// 设备型号(可选)
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 设备固件版本(可选)
        /// </summary>
        public string Firmware { get; set; }
        /// <summary>
        /// 视频输入通道数(可选)
        /// </summary>
        public int Channel { get; set; }
        /// <summary>
        /// 设备上报过DEVICEINFO
        /// </summary>
        public bool Reported { get; set; }
        /// <summary>
        /// Catalog上报视频通道数
        /// </summary>
        public int CatalogChannel { get; set; }
        /// <summary>
        /// 上次获取Catalog时间
        /// </summary>
        public DateTime GetCatalogTime { get; set; }
        /// <summary>
        /// 在线状态
        /// </summary>
        public bool Online { get; set; }
        /// <summary>
        /// 上次上线时间
        /// </summary>
        public DateTime OnlineTime { get; set; }
        /// <summary>
        /// 上次心跳时间
        /// </summary>
        public DateTime KeepAliveTime { get; set; }
        /// <summary>
        /// 离线时间
        /// </summary>
        public DateTime OfflineTime { get; set; }
        /// <summary>
        /// 远端连接信息
        /// </summary>
        public string RemoteInfo { get; set; }
        /// <summary>
        /// 是否在线(状态查询应答)
        /// </summary>
        public string DsOnline { get; set; }
        /// <summary>
        /// 是否正常工作(状态查询应答)
        /// </summary>
        public string DsStatus { get; set; }
        /// <summary>
        /// 不正常工作原因
        /// </summary>
        public string DsReason { get; set; }
        /// <summary>
        /// 是否编码
        /// </summary>
        public string DsEncode { get; set; }
        /// <summary>
        /// 是否录像
        /// </summary>
        public string DsRecord { get; set; }
        /// <summary>
        /// 设备时间和日期
        /// </summary>
        public string DsDeviceTime { get; set; }
        /// <summary>
        /// 上次设备状态信息查询应答时间
        /// </summary>
        public DateTime GetDsTime { get; set; }
        /// <summary>
        /// 是否有报警
        /// </summary>
        public bool HasAlarm { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpTime { get; set; }
    }
}
