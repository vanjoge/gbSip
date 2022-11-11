using GB28181.Client;
using GB28181.XML;
using Newtonsoft.Json.Linq;
using SipServer.Cascade;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class ChannelList : NotifyChangeDictionaryFixRef<string, GBChannel, Channel>
    {
        private GBClient gbClient;

        public ChannelList(GBClient gbClient)
        {
            this.gbClient = gbClient;
        }

        public ICollection<string> Keys => dit.Keys;


        protected override void OnFixAdd(GBChannel fixItem)
        {
            if (gbClient.sipServer.Cascade.ditWaitBindChannel.Remove(fixItem.Data.DeviceId, fixItem.Data.ChannelId, out var lst))
            {
                foreach (var cascadeChannel in lst)
                {
                    cascadeChannel.GBChannel = fixItem;
                    fixItem.ditCascadeChannel[cascadeChannel.SuperiorId] = cascadeChannel;
                    cascadeChannel.ChangeOnline(true);
                }
            }
        }

        protected override void OnFixRemove(GBChannel fixItem)
        {
            foreach (var kv in fixItem.ditCascadeChannel)
            {
                var cascadeChannel = kv.Value;
                cascadeChannel.ChangeOnline(false);
                cascadeChannel.GBChannel = null;
            }
            fixItem.ditCascadeChannel.Clear();
        }

        protected override string GetKey(Channel item)
        {
            return item.ChannelId;
        }

        protected override GBChannel NewValue(Channel item)
        {
            var value = new GBChannel();
            ChangeValue(value, item);
            return value;
        }

        protected override void ChangeValue(GBChannel fixItem, Channel item)
        {
            fixItem.Data = item;
        }

        public List<Channel> ToList()
        {
            List<Channel> lst = new List<Channel>();
            foreach (var item in dit)
            {
                lst.Add(item.Value.Data);

            }
            return lst;
        }

    }
}
