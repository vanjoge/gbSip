using GB28181.XML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class ChannelList : ConcurrentDictionary<string, Channel>
    {

        public int AddTimes { get; protected set; }
        public void AddOrUpdate(Channel item)
        {
            this[item.ChannelId] = item;
            AddTimes++;
        }
    }
}
