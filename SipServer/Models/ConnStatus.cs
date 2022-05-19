using System;
using System.Collections.Generic;
using System.Text;

namespace SipServer.Models
{
    public class ConnStatus
    {
        public ConnStatus()
        {
            CreateTime = DateTime.Now;
        }
        public bool Online { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime OnlineTime { get; set; }
        public DateTime? OfflineTime { get; set; }
        /// <summary>
        /// 上次正常通信时间
        /// </summary>
        public DateTime KeepAliveTime { get; set; }
    }
}
