using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml.Serialization;

namespace GB28181.XML
{
    /// <summary>
    /// 语音广播应答
    /// </summary>
    [XmlRoot("Response")]
    public class BroadcastResponse : XmlBase
    {
        /// <summary>
        /// 命令类型: 设备控制(必选)
        /// </summary>
        [XmlElement(nameof(CmdType))]
        public CommandType CmdType { get; set; }

        /// <summary>
        /// 命令序列号(必选)
        /// </summary>
        [XmlElement(nameof(SN))]
        public int SN { get; set; }

        /// <summary>
        /// 目标设备/区域/系统的编码,取值与目录查询请求相同(必选)
        /// </summary>
        [XmlElement(nameof(DeviceID))]
        public string DeviceID { get; set; }

        /// <summary>
        /// 查询结果标志(必选)
        /// </summary>
        [XmlElement(nameof(Result))]
        public string Result { get; set; }
    }
}