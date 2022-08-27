using DnsClient.Protocol;
using GB28181.XML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Utilities.Collections;
using SipServer.DBModel;
using SipServer.Models;
using SQ.Base;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;
using static SIPSorcery.Net.Mjpeg;
using static SIPSorcery.Net.SrtpCipherF8;

namespace SipServer.DB
{
    public class DBInfo
    {
        class AllNewObjectPool
        {
            DbContextOptions<gbsContext> options;
            public AllNewObjectPool(string ConnStr)
            {
                var build = new DbContextOptionsBuilder(new DbContextOptions<gbsContext>()).UseMySql(ConnStr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7-mysql"));

                options = build.Options as DbContextOptions<gbsContext>;
            }
            public gbsContext Get()
            {
                return new gbsContext(options);
            }

            public void Return(gbsContext dbContext)
            {

            }
        }
        SipServer sipServer;
        AllNewObjectPool dbContextPool;
        public DBInfo(SipServer sipServer)
        {
            this.sipServer = sipServer;
            //this.dbContextPool = new DefaultObjectPoolProvider().Create<gbsContext>();
            this.dbContextPool = new AllNewObjectPool(sipServer.Settings.MysqlConnectionString);
        }


        //string GetDevInfoHead(string DeviceID)
        //{
        //    return RedisConstant.DevInfoHead + DeviceID;
        //}
        ///// <summary>
        ///// 保存连接状态
        ///// </summary>
        ///// <param name="DeviceID"></param>
        ///// <param name="Status"></param>
        ///// <returns></returns>
        //public Task<bool> SaveConnStatus(string DeviceID, ConnStatus Status)
        //{
        //    return RedisHelper.HashSetAsync(GetDevInfoHead(DeviceID), RedisConstant.StatusKey, Status);
        //}
        /// <summary>
        /// 保存获取的设备状态
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>
        public async Task<bool> SaveDeviceStatus(string DeviceID, DeviceStatus deviceStatus)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var old = await GetDeviceInfo(DeviceID);
                if (old != null)
                {
                    old.DsOnline = deviceStatus.Online;
                    old.DsStatus = deviceStatus.Status;
                    old.DsReason = deviceStatus.Reason;
                    old.DsEncode = deviceStatus.Encode;
                    old.DsRecord = deviceStatus.Record;
                    old.DsDeviceTime = deviceStatus.DeviceTime;
                    old.GetDsTime = DateTime.Now;
                    dbContext.TDeviceInfos.Update(old);
                    return await dbContext.SaveChangesAsync() > 0;
                }
                return false;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        ///// <summary>
        ///// 一次获取设备、通道、状态和连接状态信息
        ///// </summary>
        ///// <param name="DeviceID"></param>
        ///// <returns></returns>
        //public async Task<(DeviceInfo, List<Catalog.Item>, DeviceStatus, ConnStatus)> GetDevAll(string DeviceID)
        //{
        //    (DeviceInfo, List<Catalog.Item>, DeviceStatus, ConnStatus) ret = default;
        //    var gbdevs = await RedisHelper.HashGetAllAsync(RedisConstant.DevInfoHead + DeviceID);
        //    foreach (var entry in gbdevs)
        //    {
        //        if (entry.Name == RedisConstant.DeviceInfoKey && entry.Value.HasValue)
        //        {
        //            ret.Item1 = TryParseJSON<DeviceInfo>(entry.Value);
        //        }
        //        else if (entry.Name == RedisConstant.ChannelsKey && entry.Value.HasValue)
        //        {
        //            ret.Item2 = TryParseJSON<List<Catalog.Item>>(entry.Value);
        //        }
        //        else if (entry.Name == RedisConstant.DeviceStatusKey && entry.Value.HasValue)
        //        {
        //            ret.Item3 = TryParseJSON<DeviceStatus>(entry.Value);
        //        }
        //        else if (entry.Name == RedisConstant.StatusKey)
        //        {
        //            ret.Item4 = TryParseJSON<ConnStatus>(entry.Value);
        //        }
        //    }
        //    return ret;
        //}


        #region Channel
        /// <summary>
        /// 获取设备通道列表
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <returns></returns>
        public async Task<List<TCatalog>> GetChannelList(string DeviceID)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                return await dbContext.TCatalogs.Where(p => p.Did == DeviceID).ToListAsync();
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }


        Task<int> DeleteChannelsByDid(gbsContext dbContext, string Did)
        {
            return dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_Catalog WHERE DID = '{Did}';");
        }
        /// <summary>
        /// 保存通道
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public async Task<bool> SaveChannels(TDeviceInfo DeviceInfo, List<TCatalog> lst)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                if (DeviceInfo.CatalogChannel != lst.Count)
                {
                    DeviceInfo.CatalogChannel = lst.Count;
                    DeviceInfo.UpTime = DateTime.Now;
                    dbContext.TDeviceInfos.Update(DeviceInfo);
                }
                await DeleteChannelsByDid(dbContext, DeviceInfo.Did);
                await dbContext.TCatalogs.AddRangeAsync(lst);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        #endregion

        #region DeviceInfo
        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <returns></returns>
        public async Task<TDeviceInfo> GetDeviceInfo(string DeviceID)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var lst = await dbContext.TDeviceInfos.Where(p => p.Did == DeviceID).Take(1).ToListAsync();
                if (lst.Count == 0)
                {
                    return null;
                }
                else
                {
                    return lst[0];
                }
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        /// <summary>
        /// 获取设备列表(支持分页)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<TDeviceInfo>> GetDeviceInfoList(bool onlyOnline, int start = 0, int count = -1)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                IQueryable<TDeviceInfo> data;
                if (onlyOnline)
                {
                    data = dbContext.TDeviceInfos.Where(p => p.Online);
                }
                else
                {
                    data = dbContext.TDeviceInfos;
                }
                data = data.Skip(start);
                if (count >= 0)
                {
                    return await data.Take(count).ToListAsync();
                }
                else
                {
                    return await data.ToListAsync();
                }
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        /// <summary>
        /// 删除设备信息
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="removeClient"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDeviceInfo(string DeviceID, bool removeClient = true)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                if (removeClient)
                    sipServer.RemoveClient(DeviceID, false);
                dbContext.TDeviceInfos.Remove(new TDeviceInfo
                {
                    Did = DeviceID
                });
                await DeleteChannelsByDid(dbContext, DeviceID);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        ///// <summary>
        ///// 仅获取设备ID(支持分页)
        ///// </summary>
        ///// <param name="start"></param>
        ///// <param name="end"></param>
        ///// <returns></returns>
        //public Task<List<string>> GetDeviceIds(long start = 0, long end = -1)
        //{
        //    return RedisHelper.GetDatabase().SortedSetRangeByRankAsync(RedisConstant.DeviceIdsKey, start, end).ContinueWith(lst =>
        //    {
        //        return lst.Result.Select(p => p.ToString()).ToList();
        //    }); ;
        //}

        public async Task<bool> AddDeviceInfo(TDeviceInfo deviceInfo)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                deviceInfo.UpTime = DateTime.Now;
                await dbContext.TDeviceInfos.AddAsync(deviceInfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        /// <summary>
        /// 保存设备信息
        /// </summary>
        /// <param name="deviceInfo"></param>
        public async Task<bool> SaveDeviceInfo(TDeviceInfo deviceInfo)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                deviceInfo.UpTime = DateTime.Now;
                dbContext.TDeviceInfos.Update(deviceInfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }

            //var old = await GetDeviceInfo(deviceInfo.DeviceID);
            //if (old != null)
            //{
            //    old.Channel = deviceInfo.Channel;
            //    old.DeviceName = deviceInfo.DeviceName;
            //    old.Manufacturer = deviceInfo.Manufacturer;
            //    old.Model = deviceInfo.Model;
            //    old.Firmware = deviceInfo.Firmware;
            //    dbContext.Update(old);
            //}
            //else
            //{
            //    TDeviceInfo device = new TDeviceInfo
            //    {
            //        Did = deviceInfo.DeviceID,
            //        Channel = deviceInfo.Channel,
            //        DeviceName = deviceInfo.DeviceName,
            //        Manufacturer = deviceInfo.Manufacturer,
            //        Model = deviceInfo.Model,
            //        Firmware = deviceInfo.Firmware,
            //    };

            //    await dbContext.AddAsync(old);
            //}
            //await dbContext.SaveChangesAsync();
        }

        #endregion


        #region SuperiorInfo
        public async Task<TSuperiorInfo> GetSuperiorInfo(string id)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var lst = await dbContext.TSuperiorInfos.Where(p => p.Id == id).Take(1).ToListAsync();
                if (lst.Count == 0)
                {
                    return null;
                }
                else
                {
                    return lst[0];
                }
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        public async Task<List<SuperiorInfoEx>> GetSuperiorList()
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var superiors = await dbContext.TSuperiorInfos.ToListAsync();
                List<SuperiorInfoEx> lst = new List<SuperiorInfoEx>();
                foreach (var item in superiors)
                {
                    var si = new SuperiorInfoEx()
                    {
                        superiorInfo = item,
                        Client = sipServer.Cascade.GetClient(item.Id),
                    };
                    lst.Add(si);
                }
                return lst;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        public async Task<bool> AddSuperior(TSuperiorInfo sinfo)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                await dbContext.TSuperiorInfos.AddAsync(sinfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        public async Task<bool> UpdateSuperior(TSuperiorInfo sinfo)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                dbContext.TSuperiorInfos.Update(sinfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        public async Task<bool> DeleteSuperior(string id)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                dbContext.TSuperiorInfos.Remove(new TSuperiorInfo
                {
                    Id = id,
                });
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        #endregion
    }
}
