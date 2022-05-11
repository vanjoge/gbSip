using GB28181.Enums;
using System.Xml.Serialization;

namespace GB28181.XML
{
    /// <summary>
    /// 状态信息报送
    /// </summary>
    [XmlRoot("Notify")]
    public class KeepAlive : XmlBase
    {

        /// <summary>
        /// 命令类型:设备状态信息报送(必选)
        /// </summary>
        [XmlElement("CmdType")]
        public CommandType CmdType { get; set; }

        /// <summary>
        /// 命令序列号(必选)
        /// </summary>
        [XmlElement("SN")]
        public int SN { get; set; }

        /// <summary>
        /// 源设备的设备/系统编码(必选)
        /// </summary>
        [XmlElement("DeviceID")]
        public string DeviceID { get; set; }

        /// <summary>
        /// 是否正常工作(必选)
        /// </summary>
        [XmlElement("Status")]
        public string Status { get; set; }
    }
}
