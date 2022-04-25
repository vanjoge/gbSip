﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using GB28181.XML;
using SQ.Base;

namespace SipServer
{
    [Serializable]
    public class Setting
    {
        /// <summary>
        /// 启用SIP日志
        /// </summary>
        public bool EnableSipLog { get; set; }

        /// <summary>
        /// 本机对外服务IP
        /// </summary>
        [DisplayName("本机对外服务IP")]
        public string ServerIP { get; set; }

        /// <summary>
        /// SIP端口
        /// </summary>
        [DisplayName("SIP端口")]
        public int SipPort { get; set; }

        /// <summary>
        /// redis连接字符串
        /// </summary>
        [DisplayName("redis连接字符串")]
        public string RedisExchangeHosts { get; set; }
        /// <summary>
        /// 离线超时时间
        /// </summary>
        [DisplayName("离线超时时间")]
        public double KeepAliveTimeoutSec { get; set; }

        public Setting()
        {
            ServerIP = "127.0.0.1";
            SipPort = 5060;
            RedisExchangeHosts = "127.0.0.1:6379,connectTimeout=20000,syncTimeout=20000,responseTimeout=20000,defaultDatabase=1,password=";
            KeepAliveTimeoutSec = 180;
#if DEBUG
            EnableSipLog = true;
#endif
        }

    }
}
