using System;
using System.Collections.Generic;
using System.Text;

namespace SipServer.Models
{
    public class ConnStatus
    {
        public bool Online { get; set; }

        public DateTime OnlineTime { get; set; }
        public DateTime OfflineTime { get; set; }
    }
}
