using GB28181.XML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipServer.Models
{
    public class DeviceList
    {
        private ConcurrentDictionary<string, CatalogItemExtend> data = new ConcurrentDictionary<string, CatalogItemExtend>();
        private ConcurrentDictionary<byte, string> ditChannelKey = new ConcurrentDictionary<byte, string>();

        public int Count => data.Count;


        public bool TryGetChannel(string DeviceID, out byte channel)
        {
            if (data.TryGetValue(DeviceID, out var item))
            {
                channel = item.Channel;
                return true;
            }
            channel = 0;
            return false;
        }
        public bool TryGetCHID(byte channel1078, out string chid)
        {
            if (data.Count >= channel1078)
            {
                chid = ditChannelKey[channel1078];
                return true;
            }
            chid = null;
            return false;
        }
        public void AddOrUpdate(CatalogItemExtend item)
        {
            data[item.Item.DeviceID] = item;
            ditChannelKey[item.Channel] = item.Item.DeviceID;
        }
        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="item"></param>
        public void AddOrUpdateDeviceList(Catalog.Item item)
        {
            data.AddOrUpdate(item.DeviceID, key =>
            {
                var Channel = (byte)(data.Count + 1);
                ditChannelKey[Channel] = item.DeviceID;
                return new CatalogItemExtend
                {
                    Item = item,
                    Channel = Channel,
                };
            },
            (key, old) =>
            {
                old.Item = item;
                return old;
            });

        }

        public List<CatalogItemExtend> ToList()
        {
            return data.Values.ToList();
        }
    }
    public class CatalogItemExtend
    {
        public Catalog.Item Item;
        /// <summary>
        /// 对应1078通道号
        /// </summary>
        public byte Channel;
    }
}
