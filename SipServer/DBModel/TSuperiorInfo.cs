using System;
using System.Collections.Generic;

namespace SipServer.DBModel
{
    public partial class TSuperiorInfo
    {
        /// <summary>
        /// 启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 上级国标编码
        /// </summary>
        public string ServerId { get; set; }
        /// <summary>
        /// 服务域
        /// </summary>
        public string ServerRealm { get; set; }
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
        public string ClientId { get; set; }
        /// <summary>
        /// 本地SIP名称
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// SIP认证用户名
        /// </summary>
        public string Sipusername { get; set; }
        /// <summary>
        /// SIP认证密码
        /// </summary>
        public string Sippassword { get; set; }
        /// <summary>
        /// 注册有效期
        /// </summary>
        public int Expiry { get; set; }
        /// <summary>
        /// 注册间隔
        /// </summary>
        public int RegSec { get; set; }
        /// <summary>
        /// 心跳周期
        /// </summary>
        public int HeartSec { get; set; }
        /// <summary>
        /// 最大心跳超时次数
        /// </summary>
        public int HeartTimeoutTimes { get; set; }
        /// <summary>
        /// TCP/UDP
        /// </summary>
        public bool UseTcp { get; set; }
    }
}
