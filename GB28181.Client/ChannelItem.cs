using GB28181.XML;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GB28181.Client
{
    public class ChannelItem
    {

        public ChannelItem(Catalog.Item CatalogItem)
        {
            this.CatalogItem = CatalogItem;
            this.Status = new DeviceStatus
            {
                DeviceID = CatalogItem.DeviceID,
                //Status = Status ? "OK" : "ERROR",
                Status = "OK",
            };
        }
        public Catalog.Item CatalogItem { get; }
        public DeviceStatus Status { get; }

        public void ChangeOnline(bool Online)
        {
            if (Online)
            {
                this.Status.Online = "ONLINE";
                this.CatalogItem.Status = "ON";
            }
            else
            {
                this.Status.Online = "OFFLINE";
                this.CatalogItem.Status = "OFF";
            }
        }
    }
}
