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
        /// SIP服务器ID
        /// </summary>
        [DisplayName("SIP服务器ID")]
        public string SipServerID { get; set; }
        /// <summary>
        /// SIP注册密码 为空表示不验证
        /// </summary>
        [DisplayName("SIP注册密码")]
        public string SipPassword { get; set; }
        /// <summary>
        /// SIP用户名 为空表示取上报
        /// </summary>
        [DisplayName("SIP用户名")]
        public string SipUsername { get; set; }
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
        /// <summary>
        /// RTVS接口地址
        /// </summary>
        [DisplayName("RTVS接口地址")]
        public string RTVSAPI { get; set; }
        /// <summary>
        /// RTVS视频服务地址
        /// </summary>
        [DisplayName("RTVS视频服务地址")]
        public string RTVSVideoServer { get; set; }
        /// <summary>
        /// RTVS视频服务端口
        /// </summary>
        [DisplayName("RTVS视频服务端口")]
        public int RTVSVideoPort { get; set; }
        /// <summary>
        /// Mysql连接字符串
        /// </summary>
        [DisplayName("Mysql连接字符串")]
        public string MysqlConnectionString { get; set; }


        public Setting()
        {
            //SipServerID = "51010100492007000001";
            SipPort = 5060;
            RedisExchangeHosts = "127.0.0.1:6379,connectTimeout=20000,syncTimeout=20000,responseTimeout=20000,defaultDatabase=0,password=";
            MysqlConnectionString = "Database=gbs;Data Source=127.0.0.1;port=3306;User Id=rtvsweb;Password=rtvs2018;charset=utf8;pooling=true";
            KeepAliveTimeoutSec = 180;
            RTVSVideoPort = 17000;
#if DEBUG
            EnableSipLog = true;
#endif
        }

    }
}
