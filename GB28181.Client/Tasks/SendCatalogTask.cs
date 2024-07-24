using GB28181.XML;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB28181.Client.Tasks
{
    internal class SendCatalogTask<T> where T : ChannelItem
    {
        private Catalog catalog;
        private List<Catalog.Item> waitSend;
        public int CSeq;
        private GB28181SipClient<T> client;

        public SendCatalogTask(GB28181SipClient<T> client, Catalog catalog, List<Catalog.Item> waitSend, int cSeq)
        {
            this.client = client;
            this.catalog = catalog;
            this.waitSend = waitSend;
            CSeq = cSeq;
        }

        public Task DoSend()
        {
            var item = waitSend[0];
            waitSend.RemoveAt(0);
            catalog.DeviceList.Clear();
            catalog.DeviceList.Add(item);
            return client.SendCatalog(CSeq, catalog);
        }
        public bool NotEmpty()
        {
            return waitSend.Count > 0;
        }
    }
}