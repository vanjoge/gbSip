namespace SipServer.Models.JT
{
    /// <summary>
    /// 
    /// </summary>
    public class JTKey
    {
        /// <summary>
        /// Sim卡号(JT1078)
        /// </summary>
        public string JTSim { get; set; }
        /// <summary>
        /// 通道号(JT1078)
        /// </summary>
        public int JTChannel { get; set; }
        /// <summary>
        /// JT1078版本 1:2019/其他 2013
        /// </summary>
        public int JTVer { get; set; }
        /// <summary>
        /// GB28181设备ID
        /// </summary>
        public string GBDeviceId { get; set; }
        /// <summary>
        /// GB28181通道号
        /// </summary>
        public string GBChannelId { get; set; }
    }
}