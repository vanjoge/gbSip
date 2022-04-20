using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml.Serialization;

namespace GB28181.XML
{
    /// <summary>
    /// 文件目录检索请求
    /// </summary>
    [XmlRoot("Query")]
    public class RecordInfo : XmlBase
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
        /// 录像开始时间(必选)
        /// </summary>
        [XmlElement(nameof(StartTime))]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 录像终止时间(必选)
        /// </summary>
        [XmlElement(nameof(EndTime))]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 文件路径名(可选)
        /// </summary>
        [XmlElement(nameof(FilePath))]
        public string FilePath { get; set; }

        /// <summary>
        /// 录像地址(可选 支持不完全查询)
        /// </summary>
        [XmlElement(nameof(Address))]
        public string Address { get; set; }

        /// <summary>
        /// 保密属性(可选)
        /// <para> 省缺值0; 0: 不涉密; 1: 涉密 </para>
        /// </summary>
        [XmlElement(nameof(Secrecy))]
        public int Secrecy { get; set; }

        /// <summary>
        /// 录像产生类型(可选)
        /// <para> time 或 alarm 或 manual 或 all </para>
        /// </summary>
        [XmlElement(nameof(Type))]
        public string Type { get; set; }

        /// <summary>
        /// 录像触发者(可选)
        /// </summary>
        [XmlElement(nameof(RecorderID))]
        public string RecorderID { get; set; }

        /// <summary>
        /// <remark> 录像模糊查询属性(可选)缺省为0; 0:不进行模糊查询,此时根据 SIP 消息中 To头域 URI中的ID值确定查询录像位置,若ID值为本域系统ID
        /// 则进行中心历史记录检索,若为前端设备ID则进行前端 设备历史记录检索;1:进行模糊查询,此时设备所在域应 同时进行中心 检索和前端检索并将结果统一返回。 </remark>
        /// </summary>
        [XmlElement(nameof(IndistinctQuery))]
        public string IndistinctQuery { get; set; }
    }
}