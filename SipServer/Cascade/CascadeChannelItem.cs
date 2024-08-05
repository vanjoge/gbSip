using GB28181.Client;
using GB28181.XML;
using SipServer.JT2GB;
using SipServer.Models;

namespace SipServer.Cascade
{
    /// <summary>
    /// 级联通道项
    /// </summary>
    public class CascadeChannelItem : ChannelItem
    {
        /// <summary>
        /// 级联KEY GUID
        /// </summary>
        public string SuperiorId;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId;
        /// <summary>
        /// 真实通道ID 如果上报自定义了会包含在CatalogItem.DeviceID里
        /// </summary>
        public string ChannelId;
        /// <summary>
        /// 设备对象
        /// </summary>
        public GBChannel GBChannel;
        /// <summary>
        /// 1078对象 不为null时表示此通道为1078转28181模式
        /// </summary>
        public JT2GBChannel J2GChannel;

        public CascadeChannelItem(string superiorId, string deviceId, string channelId, Catalog.Item catalogItem, JT2GBChannel j2gChannel) : base(catalogItem)
        {
            this.SuperiorId = superiorId;
            this.ChannelId = channelId;
            this.DeviceId = deviceId;
            this.J2GChannel = j2gChannel;
        }
    }
}
