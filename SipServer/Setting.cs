using System;
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

        public Setting()
        {
            ServerIP = "127.0.0.1";
            SipPort = 5060;
        }

    }
}
