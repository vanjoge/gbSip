using GB28181.XML;
using SipServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.Models
{
    public class DeviceInfoExt
    {
        public DeviceInfo Device { get; set; }
        public ConnStatus Status { get; set; }
    }
}
