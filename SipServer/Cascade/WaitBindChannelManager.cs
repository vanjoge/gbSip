using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SipServer.Cascade
{
    public class WaitBindChannelManager
    {
        protected Dictionary<string, List<CascadeChannelItem>> dit = new Dictionary<string, List<CascadeChannelItem>>();

        object lck = new object();
        public void Add(CascadeChannelItem item)
        {
            lock (lck)
            {
                var key = item.DeviceId + "_" + item.ChannelId;
                if (!dit.TryGetValue(key, out var lst))
                {
                    dit[key] = lst = new List<CascadeChannelItem>();
                }
                lst.Add(item);
            }
        }
        public void Remove(CascadeChannelItem item)
        {
            lock (lck)
            {
                var key = item.DeviceId + "_" + item.ChannelId;
                if (!dit.TryGetValue(key, out var lst))
                {
                    dit[key] = lst = new List<CascadeChannelItem>();
                }
                lst.Remove(item);
            }
        }
        public void Update(CascadeChannelItem old, CascadeChannelItem item)
        {
            lock (lck)
            {
                var key = item.DeviceId + "_" + item.ChannelId;
                if (!dit.TryGetValue(key, out var lst))
                {
                    dit[key] = lst = new List<CascadeChannelItem>();
                }
                lst.Remove(old);
                lst.Add(item);
            }
        }
        public bool TryGetValue(string DeviceId, string ChannelId, out List<CascadeChannelItem> lst) => dit.TryGetValue(DeviceId + "_" + ChannelId, out lst);
        public bool Remove(string DeviceId, string ChannelId, out List<CascadeChannelItem> lst) => dit.Remove(DeviceId + "_" + ChannelId, out lst);
    }
}
