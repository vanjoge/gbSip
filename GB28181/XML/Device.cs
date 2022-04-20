using GB28181.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GB28181.XML
{
    /// <summary>
    /// 设备控制
    /// </summary>
    [XmlRoot("Control")]
    public class DeviceControl : XmlBase
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
        /// 球机/云台控制命令(可选, 控制码应符合附录A中的A.3中的规定)
        /// </summary>
        [XmlElement(nameof(PTZCmd))]
        public string PTZCmd { get; set; }

        /// <summary>
        /// 远程控制命令(可选)
        /// </summary>
        [XmlElement(nameof(TeleBoot))]
        public string TeleBoot { get; set; }

        /// <summary>
        /// 录像控制命令(可选)
        /// </summary>
        [XmlElement(nameof(RecordCmd))]
        public string RecordCmd { get; set; }

        /// <summary>
        /// 报警布防/撤防命令(可选)
        /// </summary>
        [XmlElement(nameof(GuardCmd))]
        public string GuardCmd { get; set; }

        /// <summary>
        /// 报警复位命令(可选)
        /// </summary>
        [XmlElement(nameof(AlarmCmd))]
        public string AlarmCmd { get; set; }

        /// <summary>
        /// 强制关闭帧命令, 设备收到此命令应立即发送一个IDR帧(可选)
        /// </summary>
        [XmlElement(nameof(IFameCmd))]
        public string IFameCmd { get; set; }

        /// <summary>
        /// 拉框放大控制命令(可选)
        /// </summary>
        [XmlElement(nameof(DragZoomIn))]
        public DragZoomBody DragZoomIn { get; set; }

        /// <summary>
        /// 拉框缩小控制命令(可选)
        /// </summary>
        [XmlElement(nameof(DragZoomOut))]
        public DragZoomBody DragZoomOut { get; set; }

        /// <summary>
        /// 看守位控制命令(可选)
        /// </summary>
        [XmlElement(nameof(HomePosition))]
        public HomePositionBody HomePosition { get; set; }

        /// <summary>
        /// 报警复位控制时, 扩展此项, 携带报警方式, 报警类型
        /// </summary>
        [XmlElement("Info")]
        public AlarmInfoBody AlarmInfo { get; set; }

        /// <summary>
        /// 扩展信息, 可多项
        /// </summary>
        [XmlElement("Info")]
        public Catalog.Info Info { get; set; }

        public class DragZoomBody
        {
            /// <summary>
            /// 播放窗口长度像素值(必选)
            /// </summary>
            [XmlElement(nameof(Length))]
            public int Length { get; set; }

            /// <summary>
            /// 播放窗口宽度像素值(必选)
            /// </summary>
            [XmlElement(nameof(Width))]
            public int Width { get; set; }

            /// <summary>
            /// 拉框中心的横轴坐标像素值(必选)
            /// </summary>
            [XmlElement(nameof(MidPointX))]
            public int MidPointX { get; set; }

            /// <summary>
            /// 拉框中心的纵轴坐标像素值(必选)
            /// </summary>
            [XmlElement(nameof(MidPointY))]
            public int MidPointY { get; set; }

            /// <summary>
            /// 拉框长度像素值(必选)
            /// </summary>
            [XmlElement(nameof(LengthX))]
            public int LengthX { get; set; }

            /// <summary>
            /// 拉框狂赌像素值(必选)
            /// </summary>
            [XmlElement(nameof(LengthY))]
            public int LengthY { get; set; }
        }

        public class HomePositionBody
        {
            /// <summary>
            /// 看守位使能(必选)
            /// 1: 开启
            /// 0: 关闭
            /// </summary>
            [XmlElement(nameof(Enable))]
            public int Enable { get; set; }

            /// <summary>
            /// 自动归位时间间隔, 开启看守位时使用, 单位: 秒(s)(可选)
            /// </summary>
            [XmlElement(nameof(ResetTime))]
            public int ResetTime { get; set; }

            /// <summary>
            /// 调用预置位编号, 开启看守位时使用, 取值范围0~255(可选)
            /// </summary>
            [XmlElement(nameof(PresetIndex))]
            public int PresetIndex { get; set; }
        }

        public class AlarmInfoBody
        {
            /// <summary>
            /// 复位报警的报警方式树属性
            /// </summary>
            [XmlElement(nameof(AlarmMethod))]
            public string AlarmMethod { get; set; }

            /// <summary>
            /// 复位报警的报警类型属性
            /// </summary>
            [XmlElement(nameof(AlarmType))]
            public string AlarmType { get; set; }
        }
    }

    /// <summary>
    /// 设备状态查询
    /// </summary>
    [XmlRoot("Query")]
    public class DeviceStatus : XmlBase
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
    }

    /// <summary>
    /// 设备信息查询
    /// </summary>
    [XmlRoot("Query")]
    public class DeviceInfo : XmlBase
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
    }

    /// <summary>
    /// 设备配置查询
    /// </summary>
    [XmlRoot("Query")]
    public class ConfigDownload : XmlBase
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
        /// 查询配置参数类型(必选)
        /// <para>
        /// 可查询的配置类型包括基本参数配置: BasicParam,视频参数范围:VideoParamOpt,SVAC编码配置:SVACEncodeConfig,SVAC
        /// 解码配置:SVACDecodeConfig. 可同时查询多个配置类型,各类型以“/”分隔,可返回与查询SN值相同的多个响应,每个响应对应一个配置类型.
        /// </para>
        /// </summary>
        [XmlElement(nameof(ConfigType))]
        public string ConfigType { get; set; }
    }

    /// <summary>
    /// 设备预置位查询
    /// </summary>
    [XmlRoot("Query")]
    public class PresetQuery : XmlBase
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
    }

    /// <summary>
    /// 移动设备位置数据查询
    /// </summary>
    [XmlRoot("Query")]
    public class MobilePosition : XmlBase
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
        /// 移动设备位置信息上报时间间隔, 单位: 秒(s), 默认值5(可选)
        /// </summary>
        [XmlElement(nameof(Interval))]
        public int Interval { get; set; }
    }

    /// <summary>
    /// 设备控制应答
    /// </summary>
    [XmlRoot("Response")]
    public class DeviceControlResponse : XmlBase
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

    /// <summary>
    /// 设备信息查询应答
    /// </summary>
    [XmlRoot("Response")]
    public class DeviceInfoResponse : XmlBase
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
        /// 目标设备/区域/系统的名称(可选)
        /// </summary>
        [XmlElement(nameof(DeviceName))]
        public string DeviceName { get; set; }

        /// <summary>
        /// 查询结果(必选)
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 设备生产商(可选)
        /// </summary>
        // TODO: XML类型为 normalizedString
        [XmlElement(nameof(Manufacturer))]
        public string Manufacturer { get; set; }

        /// <summary>
        /// 设备固件版本(可选)
        /// </summary>
        [XmlElement(nameof(Firmware))]
        public string Firmware { get; set; }

        /// <summary>
        /// 视频输入通道数(可选)
        /// </summary>
        [XmlElement(nameof(Channel))]
        public int Channel { get; set; }

        /// <summary>
        /// 扩展信息, 可多项
        /// </summary>
        [XmlElement(nameof(Info))]
        public Catalog.Info Info { get; set; }
    }

    /// <summary>
    /// 设备状态信息查询应答
    /// </summary>
    [XmlRoot("Response")]
    public class DeviceStatusResponse : XmlBase
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

        /// <summary>
        /// 是否在线(必选)
        /// </summary>
        [XmlElement(nameof(Online))]
        public string Online { get; set; }

        /// <summary>
        /// 是否正常工作(必选)
        /// </summary>
        [XmlElement(nameof(Status))]
        public string Status { get; set; }

        /// <summary>
        /// 不正常工作原因(可选)
        /// </summary>
        [XmlElement(nameof(Reason))]
        public string Reason { get; set; }

        /// <summary>
        /// 是否编码(可选)
        /// </summary>
        [XmlElement(nameof(Encode))]
        public string Encode { get; set; }

        /// <summary>
        /// 是否录像(可选)
        /// </summary>
        [XmlElement(nameof(Record))]
        public string Record { get; set; }

        /// <summary>
        /// 设备时间和日期(可选)
        /// </summary>
        [XmlElement(nameof(DeviceTime))]
        public DateTime DeviceTime { get; set; }

        // TODO: 写到这里了
    }
}