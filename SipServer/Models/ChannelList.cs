using GB28181.XML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class ChannelList
    {
        SortedDictionary<string, Catalog.Item> data = new SortedDictionary<string, Catalog.Item>();
        public int Count => data.Count;

        public void AddOrUpdate(Catalog.Item item)
        {
            data[item.DeviceID] = item;
        }

        public List<Catalog.Item> ToList()
        {
            return data.Values.ToList();
        }
    }
}
