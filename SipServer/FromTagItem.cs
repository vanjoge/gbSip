using SIPSorcery.SIP;
using System;
using System.Collections.Generic;
using System.Text;

namespace SipServer
{
    public class FromTagItem
    {
        public FromTagItem()
        {
            AddTime = DateTime.Now;
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }

        public GBClient Client { get; set; }


        public SIPFromHeader From { get; set; }
        public SIPToHeader To { get; set; }
    }
}
