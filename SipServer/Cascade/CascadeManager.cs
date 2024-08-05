using GB28181;
using SipServer.DBModel;
using SipServer.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.Cascade
{
    public class CascadeManager
    {
        /// <summary>
        /// 客户端
        /// </summary>
        protected ConcurrentDictionary<string, CascadeClient> ditClient = new ConcurrentDictionary<string, CascadeClient>();
        /// <summary>
        /// GroupId对应客户端列表
        /// </summary>
        protected ConcurrentDictionary<string, List<CascadeClient>> ditGroupClients = new ConcurrentDictionary<string, List<CascadeClient>>();
        protected internal SipServer sipServer;

        /// <summary>
        /// KEY 设备ID_通道号
        /// </summary>
        protected internal WaitBindChannelManager ditWaitBindChannel = new WaitBindChannelManager();
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
            {
                //TODO:临时处理方案，防止未释放端口前开始连接，并不可靠
                if (sinfo.ClientPort > 0)
                {
                    await Task.Delay(2500);
                }
                await AddClient(sinfo);
            }
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
            var groups = await sipServer.DB.GetSuperiorGroups(sinfo.Id);
            var groupIds = groups.Select(p => p.GroupId).ToList();
            var tmp = await sipServer.DB.GetSuperiorChannels(sinfo.Id, groupIds);
            var id = sinfo.ClientId;
            var ClientName = string.IsNullOrEmpty(sinfo.ClientName) ? sinfo.Name : sinfo.ClientName;
            var root = new SuperiorChannel(new TCatalog
            {
                ChannelId = id,
                DeviceId = id,
                Name = ClientName,
                Manufacturer = "RTVS",
                Model = "gbsip",
                Owner = "Owner",
                CivilCode = id.Substring(0, 6),
                Address = "Address",
                RegisterWay = 1,
                Secrecy = false,
                DType = 1,
            },
            sinfo.Id, null, null
            );
            var channels = new List<SuperiorChannel>
            {
                //添加系统目录项
                root
            };
            foreach (var item in groups)
            {
                var channel = new SuperiorChannel(new TCatalog
                {
                    ChannelId = item.GroupId,
                    DeviceId = item.GroupId,
                    Name = item.Name,
                    ParentId = string.IsNullOrEmpty(item.ParentId) ? id : item.ParentId,
                    DType = item.GroupId.GetIdType() == "215" ? 2 : 3,
                },
                sinfo.Id, null, null
                );
                if (channel.ChannelId.GetIdType() == "216" && channel.ParentId.GetIdType() == "215")
                {
                    //虚拟目录增加BusinessGroupID
                    channel.BusinessGroupId = channel.ParentId;
                }
                channels.Add(channel);
            }
            foreach (var channel in tmp)
            {
                //GB/T28181—2016 附 录 O
                // <!--若上传目录中有此设备的父设备则应填写父设备ID,若无父设备则应填写系统ID;若设备属于某虚拟组织下,则应同时填写虚拟组织ID; 各个ID之间用“/”分隔。
                if (tmp.FindIndex(p => p.ChannelId == channel.ParentId) < 0)
                {
                    //找不到父设备时，将ParentId设为系统ID
                    channel.ParentId = id;
                }
                if (!string.IsNullOrEmpty(channel.GroupId))
                {
                    //有虚拟分组时，增加虚拟分组ID
                    channel.ParentId += "/" + channel.GroupId;
                }
                channels.Add(channel);
            }
            CascadeClient client = new CascadeClient(this, sinfo.Id, SuperiorInfoEx.GetServerSipStr(sinfo), sinfo.ServerId, new GB28181.XML.DeviceInfo
            {
                DeviceID = sinfo.ClientId,
                DeviceName = ClientName,
                Manufacturer = "RTVS",
                Model = "gbsip",
                Result = "OK",
                Firmware = "v0.2"

            }, channels, authUsername: sinfo.Sipusername, password: sinfo.Sippassword, expiry: sinfo.Expiry, UserAgent: sipServer.UserAgent, EnableTraceLogs: sipServer.Settings.EnableSipLog, heartSec: sinfo.HeartSec, timeOutSec: sinfo.HeartTimeoutTimes * sinfo.HeartSec
            , localPort: sinfo.ClientPort);
            client.GroupIds = groupIds;
            client.Start();
            ditClient[client.Key] = client;
            foreach (var groupId in groupIds)
            {
                var lst = ditGroupClients.GetOrAdd(groupId, key =>
                {
                    return new List<CascadeClient>();
                });
                lst.Add(client);
            }
            return true;
        }
        bool RemoveClient(string key)
        {
            if (ditClient.TryRemove(key, out var client))
            {
                client.Stop();
                foreach (var groupId in client.GroupIds)
                {
                    if (ditGroupClients.TryGetValue(groupId, out var lst))
                    {
                        lst.Remove(client);
                    }
                }
                client.GroupIds.Clear();
                return true;
            }
            return false;
        }

        public CascadeClient GetClient(string key)
        {
            ditClient.TryGetValue(key, out var client);
            return client;
        }
        public List<CascadeClient> GetClientByGroupId(string groupId)
        {
            ditGroupClients.TryGetValue(groupId, out var lst);
            return lst;
        }
    }
}
