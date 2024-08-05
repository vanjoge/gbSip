using SipServer.DBModel;
using SipServer.Models.JT;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.JT2GB
{
    public class JT2GBManager
    {
        /// <summary>
        /// 客户端
        /// </summary>
        protected ConcurrentDictionary<string, JT2GBClient> ditClient = new ConcurrentDictionary<string, JT2GBClient>();
        protected internal SipServer sipServer;
        public JT2GBManager(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }
        public bool TryGetClient(string DeviceID, out JT2GBClient value)
        {
            if (DeviceID == null)
            {
                value = null;
                return false;
            }
            return ditClient.TryGetValue(DeviceID, out value);
        }


        public async Task AddJTItems(List<JTItem> lst)
        {
            foreach (JTItem item in lst)
            {
                var client = ditClient.GetOrAdd(item.GBDeviceId, key =>
                 {
                     return new JT2GBClient(item.GBDeviceId, this);
                 });
                var channel = client.GetOrAddChannel(item.GBChannelId, key =>
                {
                    return new JT2GBChannel(client, item.GBChannelId, item.JTSim, item.JTChannel, item.JTVer == 1);
                });
                if (item.GBGroupID != null)
                {
                    var lstCascadeClient = sipServer.Cascade.GetClientByGroupId(item.GBGroupID);
                    channel.Online(item.ExpiresIn, lstCascadeClient);
                    if (lstCascadeClient != null && lstCascadeClient.Count > 0)
                    {
                        foreach (var cascadeClient in lstCascadeClient)
                        {
                            cascadeClient.AddChannel(new Models.SuperiorChannel(new TCatalog
                            {
                                ChannelId = item.GBChannelId,
                                DeviceId = item.GBDeviceId,
                                Name = item.GBChannelName,
                                Manufacturer = "RTVS",
                                Model = "gbsip",
                                Owner = "Owner",
                                CivilCode = item.GBChannelId.Substring(0, 6),
                                Address = "Address",
                                RegisterWay = 1,
                                Secrecy = false,
                                DType = 1001,
                                Online = true,
                                ParentId = cascadeClient.DeviceID + "/" + item.GBGroupID,
                                Status = "ON",
                            }, cascadeClient.Key, item.GBChannelId, item.GBGroupID), channel);
                        }
                    }
                }
            }
        }
        public async Task RemoveJTItems(List<JTKey> lst)
        {
            foreach (JTKey item in lst)
            {
                if (ditClient.TryGetValue(item.GBDeviceId, out var client))
                {
                    if (client.TryRemove(item.GBChannelId, out var channel))
                    {
                        channel.Offline();
                    }
                }
            }
        }

        public void Check()
        {
            try
            {
                var clients = ditClient.Values.ToList();
                foreach (var item in clients)
                {
                    if (!item.Check())
                    {
                        ditClient.TryRemove(item.Key, out var _);
                    }
                }
            }
            catch
            {
            }
        }
    }
}