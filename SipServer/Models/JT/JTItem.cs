namespace SipServer.Models.JT
{
    /// <summary>
    /// 
    /// </summary>
    public class JTItem : JTKey
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string GBDeviceName { get; set; }
        /// <summary>
        /// GB28181通道名称
        /// </summary>
        public string GBChannelName { get; set; }
        /// <summary>
        /// 绑定分组ID
        /// </summary>
        public string GBGroupID { get; set; }
        /// <summary>
        /// 过期时间(秒) 小于等于0表示不过期
        /// </summary>
        public long ExpiresIn { get; set; }
    }
}