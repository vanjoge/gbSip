using GB28181.XML;
using SipServer.Cascade;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class SuperiorInfoEx : SuperiorInfo
    {
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public CascadeClient Client;
    }
    /// <summary>
    /// 上级平台信息
    /// </summary>
    public class SuperiorInfo
    {
        public SuperiorInfo()
        {
            Enable = true;
            ServerPort = 5060;
            Expiry = 3600;
            RegSec = 60;
            HeartSec = 60;
            HeartTimeoutTimes = 3;
        }
        /// <summary>
        /// 启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 上级国标编码
        /// </summary>
        public string ServerID { get; set; }
        /// <summary>
        /// 上级IP/域名
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 上级端口
        /// </summary>
        public int ServerPort { get; set; }
        /// <summary>
        /// 本地SIP国标编码
        /// </summary>
        public string ClientID { get; set; }
        /// <summary>
        /// 本地SIP名称
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// SIP认证用户名
        /// </summary>
        public string SIPUsername { get; set; }
        /// <summary>
        /// SIP认证密码
        /// </summary>
        public string SIPPassword { get; set; }
        /// <summary>
        /// 注册有效期
        /// </summary>
        public int Expiry { get; set; }
        /// <summary>
        /// 注册间隔
        /// </summary>
        public double RegSec { get; set; }
        /// <summary>
        /// 心跳周期
        /// </summary>
        public double HeartSec { get; set; }
        /// <summary>
        /// 最大心跳超时次数
        /// </summary>
        public int HeartTimeoutTimes { get; set; }
        /// <summary>
        /// TCP/UDP
        /// </summary>
        public bool UseTcp { get; set; }


        /// <summary>
        /// 获取SIP连接信息
        /// </summary>
        /// <returns></returns>
        public string GetServerSipStr()
        {
            return $"sip:{ServerID}@{Server}:{ServerPort}{(UseTcp ? ";transport=tcp" : "")}";
        }
    }
}
