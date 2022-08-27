using GB28181.XML;
using SipServer.DBModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class ChannelList: ConcurrentDictionary<string, TCatalog>
    {

        public int AddTimes { get; protected set; }
        public void AddOrUpdate(TCatalog item)
        {
            this[item.Did] = item;
            AddTimes++;
        }
    }
}
