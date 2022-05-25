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
        private ConcurrentDictionary<string, Catalog.Item> data = new ConcurrentDictionary<string, Catalog.Item>();
        List<Catalog.Item> lst = new List<Catalog.Item>();
        public int Count => lst.Count;

        public void AddOrUpdate(Catalog.Item item)
        {
            if (data.TryGetValue(item.DeviceID, out var old))
            {
                old.CopyVal(item);
            }
            else
            {
                lst.Add(item);
                data[item.DeviceID] = item;
            }
        }

        public List<Catalog.Item> ToList()
        {
            return lst;
        }
    }
}
