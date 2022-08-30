using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class ChannelConf
    {
        /// <summary>
        /// 远程设备终结点
        /// </summary>
        public string RemoteEp { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 网络传输模式(0 自动; 1 TCP被动; 2 TCP主动; 3 UDP)
        /// </summary>
        public int NetType { get; set; }
        /// <summary>
        /// 对讲模式(0 自动; 1 Invite被动; 2 Invite主动; 3 广播)
        /// </summary>
        public int TalkType { get; set; }
    }
}
