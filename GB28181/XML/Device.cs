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
}