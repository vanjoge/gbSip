using SipServer.DBModel;
using SipServer.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.Cascade
{
    public class CascadeManager
    {
        /// <summary>
        /// 客户端
        /// </summary>
        ConcurrentDictionary<string, CascadeClient> ditClient = new ConcurrentDictionary<string, CascadeClient>();
        protected internal SipServer sipServer;

        public CascadeManager(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }
        public async Task Start()
        {
            var lst = (await sipServer.DB.GetSuperiorList()).list;
            foreach (var item in lst)
            {
                if (item.Enable)
                    await AddClient(item);
            }
        }
        public delegate void ItemToDo(CascadeClient item);
        /// <summary>
        /// 遍历Client
        /// </summary>
        /// <param name="itemToDo"></param>
        public void EachClient(ItemToDo itemToDo)
        {
            var clients = ditClient.Values.ToList();
            foreach (var item in clients)
            {
                itemToDo(item);
            }
        }
        public void Stop()
        {
            EachClient(client =>
            {
                RemoveClient(client.Key);
            });
        }
        public async Task<bool> Add(TSuperiorInfo sinfo)
        {
            if (await sipServer.DB.AddSuperior(sinfo))
            {
                if (sinfo.Enable)
                    return await AddClient(sinfo);
            }
            return false;
        }

        public async Task<bool> Update(TSuperiorInfo sinfo)
        {
            await sipServer.DB.UpdateSuperior(sinfo);

            RemoveClient(sinfo.Id);
            if (sinfo.Enable)
                await AddClient(sinfo);
            return true;
        }
        public async Task<bool> Remove(params string[] ids)
        {
            bool flag = true;
            await sipServer.DB.DeleteSuperiors(ids);
            foreach (var id in ids)
            {
                flag = RemoveClient(id) && flag;
            }
            return flag;
        }
        async Task<bool> AddClient(TSuperiorInfo sinfo)
        {
            var channels = await sipServer.DB.GetSuperiorChannels(sinfo.Id);
            CascadeClient client = new CascadeClient(this, sinfo.Id, SuperiorInfoEx.GetServerSipStr(sinfo), sinfo.ServerId, new GB28181.XML.DeviceInfo
            {
                DeviceID = sinfo.ClientId,
                DeviceName = sinfo.ClientName,
                Manufacturer = "RTVS",
                Model = "gbsip",
                Result = "OK",
                Firmware = "v0.1"

            }, channels, authUsername: sinfo.Sipusername, password: sinfo.Sippassword, expiry: sinfo.Expiry, UserAgent: sipServer.UserAgent, EnableTraceLogs: sipServer.Settings.EnableSipLog, heartSec: sinfo.HeartSec, timeOutSec: sinfo.HeartTimeoutTimes * sinfo.HeartSec);
            client.Start();
            ditClient[client.Key] = client;
            return true;
        }
        bool RemoveClient(string key)
        {
            if (ditClient.TryRemove(key, out var client))
            {
                client.Stop();
                return true;
            }
            return false;
        }

        public CascadeClient GetClient(string key)
        {
            ditClient.TryGetValue(key, out var client);
            return client;
        }
    }
}
