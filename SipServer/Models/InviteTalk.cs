using GB28181.XML;
using SipServer.Models;
using SIPSorcery.SIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.Models
{
    /// <summary>
    /// 对讲转换策略
    /// </summary>
    public enum InviteTalk
    {
        /// <summary>
        /// 发现设备不支持时返回错误2
        /// </summary>
        Auto = 0,
        /// <summary>
        /// 不检查原样发送
        /// </summary>
        Force = 1,
        /// <summary>
        /// 发现设备不支持时直接转换为广播发送
        /// </summary>
        Transform = 2,


    }
}
