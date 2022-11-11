using GB28181.Client;
using GB28181.XML;
using SipServer.Cascade;
using SipServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SipServer.Models
{

    public class GBChannel
    {
        public ConcurrentDictionary<string, CascadeChannelItem> ditCascadeChannel = new ConcurrentDictionary<string, CascadeChannelItem>();
        public Channel Data;

        public GBChannel()
        {
        }

    }
}
