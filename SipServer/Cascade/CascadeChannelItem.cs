using GB28181.Client;
using GB28181.XML;
using SipServer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SipServer.Cascade
{

    public class CascadeChannelItem : ChannelItem
    {
        public string SuperiorId;
        public string DeviceId;
        public string ChannelId;
        public GBChannel GBChannel;

        public CascadeChannelItem(string SuperiorId, string DeviceId, string ChannelId, Catalog.Item CatalogItem) : base(CatalogItem)
        {
            this.SuperiorId = SuperiorId;
            this.ChannelId = ChannelId;
            this.DeviceId = DeviceId;
        }

    }
}
