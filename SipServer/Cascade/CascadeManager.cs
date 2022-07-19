using SipServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipServer.Cascade
{
    public class CascadeManager
    {
        /// <summary>
        /// 客户端
        /// </summary>
        ConcurrentDictionary<string, CascadeClient> ditClient = new ConcurrentDictionary<string, CascadeClient>();
        private SipServer sipServer;

        public CascadeManager(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }
        public async Task Start()
        {
            var lst = await sipServer.DB.GetSuperiorList();
            foreach (var item in lst)
            {
                if (item.Enable)
                    AddClient(item);
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
        public async Task<bool> Add(SuperiorInfo sinfo)
        {
            if (await sipServer.DB.SaveSuperior(sinfo))
            {
                if (sinfo.Enable)
                    return AddClient(sinfo);
            }
            return false;
        }

        public async Task Update(SuperiorInfo sinfo)
        {
            await sipServer.DB.SaveSuperior(sinfo);

            RemoveClient(sinfo.ID);
            if (sinfo.Enable)
                AddClient(sinfo);
        }
        public async Task<bool> Remove(string id)
        {
            await sipServer.DB.DeleteSuperior(id);
            return RemoveClient(id);
        }
        bool AddClient(SuperiorInfo sinfo)
        {
            CascadeClient client = new CascadeClient(sinfo.ID, sinfo.GetServerSipStr(), sinfo.ServerID, new GB28181.XML.DeviceInfo
            {
                DeviceID = sinfo.ClientID,
                DeviceName = sinfo.ClientName,
                Manufacturer = "RTVS",
                Model = "gbsip",
                Result = "OK",

            }, null, password: sinfo.SIPPassword, expiry: sinfo.Expiry, UserAgent: sipServer.UserAgent, EnableTraceLogs: sipServer.Settings.EnableSipLog, heartSec: sinfo.HeartSec, timeOutSec: sinfo.HeartTimeoutTimes * sinfo.HeartSec);
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
