using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml.Serialization;

namespace GB28181.XML
{
    /// <summary>
    /// 报警查询
    /// </summary>
    [XmlRoot("Query")]
    public class Alarm : XmlBase
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
        /// 报警起始级别(可选)
        /// <para> 0为全部,1为一级警情,2为二级警情,3为三级警情,4为四级警情 </para>
        /// </summary>
        [XmlElement(nameof(StartAlarmPriority))]
        public string StartAlarmPriority { get; set; }

        /// <summary>
        /// 报警终止级别(可选)
        /// <para> 0为全部,1为一级警情,2为二级警情,3为三级警情,4为四级警情 </para>
        /// </summary>
        [XmlElement(nameof(EndAlarmPriority))]
        public string EndAlarmPriority { get; set; }

        /// <summary>
        /// 报警方式条件(可选)
        /// <para> 取值0为全部,1为电话报警,2为设备报警,3为短信报警,4为GPS报警,5为视频报警,6为设备故障报警,7其他报警;可以为直接组合如12为电话报警或设备报警 </para>
        /// </summary>
        [XmlElement(nameof(AlarmMethod))]
        public string AlarmMethod { get; set; }

        /// <summary>
        /// 报警类型
        /// </summary>
        [XmlElement(nameof(AlarmType))]
        public string AlarmType { get; set; }

        /// <summary>
        /// 报警发生开始时间
        /// </summary>
        [XmlElement(nameof(StartAlarmTime))]
        public DateTime StartAlarmTime { get; set; }

        /// <summary>
        /// 报警发生结束时间
        /// </summary>
        [XmlElement(nameof(EndAlarmTime))]
        public DateTime EndAlarmTime { get; set; }
    }

    /// <summary>
    /// 报警通知应答
    /// </summary>
    [XmlRoot("Response")]
    public class AlarmResponse : XmlBase
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
        /// 执行结果标志(必选)
        /// </summary>
        [XmlElement(nameof(Result))]
        public string Result { get; set; }
    }
}