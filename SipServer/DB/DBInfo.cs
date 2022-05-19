using GB28181.XML;
using SipServer.Models;
using SQ.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipServer.DB
{
    public class DBInfo
    {
        SipServer sipServer;

        public DBInfo(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }

        public Task<List<Catalog.Item>> GetChannelList(string DeviceID)
        {
            return sipServer.RedisHelper.HashGetAsync<List<Catalog.Item>>(RedisConstant.DevInfoHead + DeviceID, RedisConstant.ChannelsKey);
        }
        public Task<DeviceInfo> GetDeviceInfo(string DeviceID)
        {
            return sipServer.RedisHelper.HashGetAsync<DeviceInfo>(RedisConstant.DevInfoHead + DeviceID, RedisConstant.DeviceInfoKey);
        }
        public async Task<List<DeviceInfoEx>> GetDeviceInfoList(long start = 0, long end = -1)
        {
            List<DeviceInfoEx> lstDev = new List<DeviceInfoEx>();
            var db = sipServer.RedisHelper.GetDatabase();
            var lst = await db.SortedSetRangeByRankAsync(RedisConstant.DeviceIdsKey, start, end);
            if (lst.Length > 0)
            {
                Dictionary<string, Task<StackExchange.Redis.RedisValue>> ditDeviceInfo = new Dictionary<string, Task<StackExchange.Redis.RedisValue>>();
                Dictionary<string, Task<StackExchange.Redis.RedisValue>> ditStatus = new Dictionary<string, Task<StackExchange.Redis.RedisValue>>();
                var bat = db.CreateBatch();
                foreach (var id in lst)
                {
                    ditDeviceInfo[id] = bat.HashGetAsync(RedisConstant.DevInfoHead + id, RedisConstant.DeviceInfoKey);
                    ditStatus[id] = bat.HashGetAsync(RedisConstant.DevInfoHead + id, RedisConstant.StatusKey);
                }
                bat.Execute();
                foreach (var item in ditDeviceInfo)
                {
                    if (item.Value.Result.HasValue)
                    {
                        var dev = item.Value.Result.ToString().ParseJSON<DeviceInfo>();
                        var status = ditStatus[item.Key].Result.ToString().ParseJSON<ConnStatus>();
                        if (status.Online && sipServer.TryGetClient(dev.DeviceID, out var client))
                        {
                            status.KeepAliveTime = client.Status.KeepAliveTime;
                        }
                        lstDev.Add(new DeviceInfoEx
                        {
                            Device = dev,
                            Status = status
                        });
                    }
                }
            }

            return lstDev;
        }

        public async Task<bool> DeleteDeviceInfo(string DeviceID, bool removeClient = true)
        {
            var db = sipServer.RedisHelper.GetDatabase();
            var bat = db.CreateTransaction();
            bat.KeyDeleteAsync(RedisConstant.DevInfoHead + DeviceID);
            bat.SortedSetRemoveAsync(RedisConstant.DeviceIdsKey, DeviceID);
            if (await bat.ExecuteAsync())
            {
                if (removeClient)
                    sipServer.RemoveClient(DeviceID);
                return true;
            }
            return false;
        }

        public async Task<List<string>> GetDeviceIds(long start = 0, long end = -1)
        {
            var lst = await sipServer.RedisHelper.GetDatabase().SortedSetRangeByRankAsync(RedisConstant.DeviceIdsKey, start, end);
            return lst.Select(p => p.ToString()).ToList();
        }
    }
}
