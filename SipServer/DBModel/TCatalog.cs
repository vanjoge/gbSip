using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class TCatalog
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
        /// 上级ID
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// 设备/区域/系统名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 当为设备时，设备厂商
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// 当为设备时，设备型号
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 当为设备时，设备归属
        /// </summary>
        public string Owner { get; set; }
        /// <summary>
        /// 行政区域
        /// </summary>
        public string CivilCode { get; set; }
        /// <summary>
        /// 警区
        /// </summary>
        public string Block { get; set; }
        /// <summary>
        /// 当为设备时，安装地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 当为设备时，是否有子设备(必选)， 1有 0没有
        /// </summary>
        public bool Parental { get; set; }
        /// <summary>
        /// 虚拟分组ID
        /// </summary>
        public string BusinessGroupId { get; set; }
        /// <summary>
        /// 信令安全模式(可选)缺省为0； 0：不采用 2：S/MIME签名方式 3：S/MIME加密签名同时采用方式 4：数字摘要方式
        /// </summary>
        public int SafetyWay { get; set; }
        /// <summary>
        /// 注册方式(必选)缺省为1； 1:符合IETF FRC 3261标准的认证注册模式； 2:基于口令的双向认证注册模式； 3:基于数字证书的双向认证注册模式；
        /// </summary>
        public int RegisterWay { get; set; }
        /// <summary>
        /// 证书序列号
        /// </summary>
        public string CertNum { get; set; }
        /// <summary>
        /// 证书有效标志(有证书的设备必选)， 0无效 1有效
        /// </summary>
        public bool Certifiable { get; set; }
        /// <summary>
        /// 无效原因码
        /// </summary>
        public int ErrCode { get; set; }
        /// <summary>
        /// 证书终止有效期
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 保密属性(必选) 0：不涉密 1涉密
        /// </summary>
        public bool Secrecy { get; set; }
        /// <summary>
        /// 设备/区域/系统IP地址
        /// </summary>
        public string Ipaddress { get; set; }
        /// <summary>
        /// 设备/区域/系统端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 设备口令
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// 远程设备终结点
        /// </summary>
        public string RemoteEp { get; set; }
        /// <summary>
        /// 在线状态
        /// </summary>
        public bool Online { get; set; }
        /// <summary>
        /// 上次上线时间
        /// </summary>
        public DateTime OnlineTime { get; set; }
        /// <summary>
        /// 离线时间
        /// </summary>
        public DateTime? OfflineTime { get; set; }
    }
}
