using Microsoft.EntityFrameworkCore;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using RedisHelp;
using SipServer.DBModel;
using SipServer.Models;
using SQ.Base;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SipServer.DB
{
    public partial class DBInfo
    {
        class AllNewObjectPool
        {
#if DEBUG
            public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
#endif
            DbContextOptions<gbsContext> options;
            public AllNewObjectPool(string ConnStr)
            {
                var build = new DbContextOptionsBuilder(new DbContextOptions<gbsContext>()).UseMySql(ConnStr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7-mysql"));

#if DEBUG
                build = build.UseLoggerFactory(MyLoggerFactory);
#endif
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
        /// <summary>
        /// Redis操作类
        /// </summary>
        public RedisHelp.RedisHelper RedisHelper { protected set; get; }
        public DBInfo(SipServer sipServer)
        {
            this.sipServer = sipServer;
            //this.dbContextPool = new DefaultObjectPoolProvider().Create<gbsContext>();
            this.dbContextPool = new AllNewObjectPool(sipServer.Settings.MysqlConnectionString);
            RedisHelper = new RedisHelp.RedisHelper(-1, sipServer.Settings.RedisExchangeHosts);
            RedisHelper.SetSysCustomKey("");
        }

        Task<T> SafeHashGetAsync<T>(string key, string dataKey)
        {
            return RedisHelper.GetDatabase().HashGetAsync(key, dataKey).ContinueWith<T>(p =>
            {
                if (p.Result.HasValue)
                {
                    return SQ.Base.JsonHelper.ParseJSON<T>(p.Result);
                }
                return default;
            });
        }

        #region Channel
        public async Task<TCatalog> GetCatalog(gbsContext dbContext, string DeviceId, string ChannelId)
        {
            var data = from u in dbContext.TCatalogs
                       where u.DeviceId == DeviceId && u.ChannelId == ChannelId
                       select u;
            var lst = await data.ToListAsync();
            if (lst.Count > 0)
            {
                return lst[0];
            }
            return null;
        }
        //public async Task<TChannel> GetChannel(string DeviceId, string ChannelId, gbsContext dbContext = null, bool OnlyDB = false)
        //{
        //    if (!OnlyDB && sipServer.TryGetClient(DeviceId, out var client) && client.TryGetChannel(ChannelId, out var old))
        //    {
        //        return old;
        //    }
        //    bool flag = dbContext == null;
        //    if (flag)
        //        dbContext = dbContextPool.Get();
        //    try
        //    {
        //        var data = from p in dbContext.TCatalogs
        //                   where p.DeviceId == DeviceId && p.DeviceId == ChannelId
        //                   select p;
        //        var lst = await data.Take(1).ToListAsync();
        //        if (lst.Count == 0)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            return lst[0];
        //        }
        //    }
        //    finally
        //    {
        //        if (flag)
        //            dbContextPool.Return(dbContext);
        //    }
        //}
        /// <summary>
        /// 获取设备通道列表
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <returns></returns>
        public async Task<DPager<Channel>> GetChannelList(string DeviceID, string ParentId = null, string ChannelId = null, string Name = null, bool? Parental = null, string Manufacturer = null, bool? Online = null, int Page = 1, int Limit = -1, bool OnlyDB = false)
        {
            var dbContext = dbContextPool.Get();
            try
            {

                //var data = from u in dbContext.TCatalogs
                //           where u.DeviceId == DeviceID
                //           select new Channel(u);

                var data = dbContext.TCatalogs.Where(p => p.DeviceId == DeviceID);


                if (!string.IsNullOrWhiteSpace(ChannelId))
                {
                    data = data.Where(p => p.ChannelId.Contains(ChannelId));
                }
                if (!string.IsNullOrWhiteSpace(ParentId))
                {
                    data = data.Where(p => p.ParentId == ParentId);
                }
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    data = data.Where(p => p.Name.Contains(Name));
                }
                if (!string.IsNullOrWhiteSpace(Manufacturer))
                {
                    data = data.Where(p => p.Manufacturer.Contains(Manufacturer));
                }
                if (Parental.HasValue)
                {
                    data = data.Where(p => p.Parental == Parental.Value);
                }
                if (Online.HasValue)
                {
                    data = data.Where(p => p.Online == Online.Value);
                }


                //根节点获取时，加载出ParentId不正确的项
                if (DeviceID == ParentId)
                {
                    var all = dbContext.TCatalogs.Where(_ => _.DeviceId == DeviceID);
                    //ParentId不存在
                    var noHaveParent = from a in all where !(from b in all select b.ChannelId).Contains(a.ParentId) select a;
                    //ParentId与ChannelId相等
                    var eq = from a in all where a.ChannelId == a.ParentId select a;

                    data = data.Union(noHaveParent).Union(eq);
                }

                //var data = dbContext.TCatalogs.Where(p => p.DeviceId == DeviceID);
                var sumData = data;
                if (Page > 1)
                {
                    data = data.Skip(GetStart(Page, Limit));
                }
                if (Limit >= 0)
                {
                    data = data.Take(Limit);
                }
                List<TCatalog> lstC = await data.ToListAsync();

                var tuple = await GetSum(Page, Limit, lstC.Count, sumData);
                int sum = tuple.Item1;
                Limit = tuple.Item2;

                List<Channel> lst = lstC.ConvertAll<Channel>(p => new Channel(p));
                //如果不在线 将状态设为离线
                bool setOff = false;
                if (!OnlyDB && !sipServer.TryGetClient(DeviceID, out var client))
                {
                    setOff = true;
                }
                //List<Channel> lstRet = new List<Channel>(lst.Count);
                //从redis取配置
                var confs = await GetChannelConfs(DeviceID, lst.Select(p => p.ChannelId));
                foreach (var item in lst)
                {
                    if (setOff)
                    {
                        item.Online = false;
                    }
                    item.SetChannelConf(confs[item.ChannelId], sipServer.Settings);
                }
                return new DPager<Channel>(lst, Page, Limit, sum);
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }

        /// <summary>
        /// 删除部分通道
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="ChannelIds"></param>
        /// <returns></returns>
        public async Task<bool> DeleteChannel(string DeviceID, string[] ChannelIds)
        {
            if (ChannelIds == null || ChannelIds.Length == 0)
            {
                return true;
            }
            var dbContext = dbContextPool.Get();
            try
            {
                string str = "";
                sipServer.TryGetClient(DeviceID, out var client);
                foreach (var ChannelId in ChannelIds)
                {
                    client?.RemoveChannel(ChannelId);
                    str += $",'{ChannelId}'";
                }
                str = str[1..];
                await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_Catalog WHERE ChannelID in ({str}) AND DeviceID ='{DeviceID}';");
                var count = await dbContext.TCatalogs.CountAsync(p => p.DeviceId == DeviceID);
                await dbContext.Database.ExecuteSqlRawAsync($"UPDATE T_DeviceInfo SET CatalogChannel = {count} WHERE DeviceID ='{DeviceID}';");
                return true;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }


        Task<int> DeleteChannelsByDid(gbsContext dbContext, string DeviceID)
        {
            return dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_Catalog WHERE DeviceID = '{DeviceID}';");
        }
        /// <summary>
        /// 保存通道
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        internal async Task<bool> SaveChannels(TDeviceInfo DeviceInfo, List<Channel> lst)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var dt = DateTime.Now;
                DeviceInfo.GetCatalogTime = dt;
                if (DeviceInfo.CatalogChannel != lst.Count)
                {
                    DeviceInfo.CatalogChannel = lst.Count;
                    DeviceInfo.UpTime = dt;
                    dbContext.TDeviceInfos.Update(DeviceInfo);
                }

                await DeleteChannelsByDid(dbContext, DeviceInfo.DeviceId);
                List<TCatalog> catalogs = new List<TCatalog>(lst.Count);
                List<ChannelConf> confList = new List<ChannelConf>();
                List<string> channelIds = new List<string>();
                foreach (var item in lst)
                {
                    catalogs.Add(item);
                    confList.Add(item.ToChannelConf());
                    channelIds.Add(item.ChannelId);
                }
                await dbContext.TCatalogs.AddRangeAsync(catalogs);
                await dbContext.SaveChangesAsync();
                await UpdateChannelsConfs(DeviceInfo.DeviceId, channelIds, confList);
                return true;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }

        public async Task<Dictionary<string, ChannelConf>> GetChannelConfs(string DeviceId, IEnumerable<string> ChannelIds)
        {
            Dictionary<string, ChannelConf> dic = new Dictionary<string, ChannelConf>();
            if (ChannelIds.Count() == 0)
            {
                return dic;
            }
            var bat = RedisHelper.GetDatabase().CreateBatch();
            Dictionary<string, Task<RedisValue>> dicTask = new Dictionary<string, Task<RedisValue>>();
            foreach (var id in ChannelIds)
            {
                dicTask[id] = bat.HashGetAsync(RedisConstant.DeviceConfKey + DeviceId, RedisConstant.ChannelKey + id);
            }
            bat.Execute();
            foreach (var tsk in dicTask)
            {
                var item = await tsk.Value;
                if (item.HasValue)
                {
                    var conf = item.ToString().ParseJSON<ChannelConf>();
                    dic[tsk.Key] = conf;
                }
                else
                {
                    dic[tsk.Key] = new ChannelConf();
                }
            }
            return dic;
        }
        public Task<ChannelConf> GetChannelConf(string DeviceId, string ChannelId)
        {
            return SafeHashGetAsync<ChannelConf>(RedisConstant.DeviceConfKey + DeviceId, RedisConstant.ChannelKey + ChannelId);
        }

        public Task UpdateChannelsConfs(string DeviceId, List<string> ChannelIds, List<ChannelConf> Channels)
        {
            var entry = new HashEntry[Channels.Count()];
            for (int i = 0; i < Channels.Count(); i++)
            {
                entry[i] = new HashEntry(ChannelIds[i], Channels[i].ToJson());
            }
            return RedisHelper.GetDatabase().HashSetAsync(RedisConstant.DeviceConfKey + DeviceId, entry);
        }
        public Task<bool> UpdateChannelConf(string DeviceId, string ChannelId, ChannelConf Channel)
        {
            if (sipServer.TryGetClient(DeviceId, out var client) && client.TryGetChannel(ChannelId, out var old))
            {
                old.Data.SetChannelConf(Channel, sipServer.Settings);
            }
            return this.RedisHelper.HashSetAsync(RedisConstant.DeviceConfKey + DeviceId, RedisConstant.ChannelKey + ChannelId, Channel);
        }


        #endregion

        #region DeviceInfo
        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="dbContext"></param>
        /// <param name="OnlyDB">仅从DB获取，为flase时会先找一遍在线列表</param>
        /// <returns></returns>
        public async Task<TDeviceInfo> GetDeviceInfo(string DeviceID, gbsContext dbContext = null, bool OnlyDB = false)
        {
            if (!OnlyDB && sipServer.TryGetClient(DeviceID, out var client))
            {
                return client.GetDeviceInfo();
            }

            bool flag = dbContext == null;
            if (flag)
                dbContext = dbContextPool.Get();
            try
            {
                var lst = await dbContext.TDeviceInfos.Where(p => p.DeviceId == DeviceID).Take(1).ToListAsync();
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
                if (flag)
                    dbContextPool.Return(dbContext);
            }
        }
        private int GetStart(int Page, int Limit)
        {
            return (Page - 1) * Limit;
        }
        /// <summary>
        /// 获取设备列表(支持分页)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<DPager<TDeviceInfo>> GetDeviceList(string DeviceId, string DeviceName, string Manufacturer, bool? Online, int Page = 1, int Limit = 10)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                IQueryable<TDeviceInfo> data;
                if (Online.HasValue)
                {
                    data = dbContext.TDeviceInfos.Where(p => p.Online == Online.Value);
                }
                else
                {
                    data = dbContext.TDeviceInfos;
                }
                if (!string.IsNullOrWhiteSpace(DeviceId))
                {
                    data = data.Where(p => p.DeviceId.Contains(DeviceId));
                }
                if (!string.IsNullOrWhiteSpace(DeviceName))
                {
                    data = data.Where(p => p.DeviceName.Contains(DeviceName));
                }
                if (!string.IsNullOrWhiteSpace(Manufacturer))
                {
                    data = data.Where(p => p.Manufacturer.Contains(Manufacturer));
                }


                var sumData = data;
                data = data.OrderByDescending(p => p.CreateTime).Skip(GetStart(Page, Limit));
                if (Limit >= 0)
                {
                    data = data.Take(Limit);
                }
                List<TDeviceInfo> lst = await data.ToListAsync();

                var tuple = await GetSum(Page, Limit, lst.Count, sumData);
                int sum = tuple.Item1;
                Limit = tuple.Item2;

                for (int i = 0; i < lst.Count; i++)
                {
                    var item = lst[i];
                    if (sipServer.TryGetClient(item.DeviceId, out var client))
                    {
                        lst[i] = client.GetDeviceInfo();
                    }
                    else
                    {
                        item.Online = false;
                    }
                }
                return new DPager<TDeviceInfo>(lst, Page, Limit, sum);
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        async Task<Tuple<int, int>> GetSum<T>(int Page, int Limit, int dataCount, IQueryable<T> sumData)
        {
            int sum;
            if ((Page == 1 && dataCount < Limit) || (Page <= 1 && Limit < 0))
            {
                sum = dataCount;
            }
            else
            {
                sum = await sumData.CountAsync();
            }
            if (Limit < 0) Limit = sum;
            return new Tuple<int, int>(sum, Limit);
        }
        /// <summary>
        /// 删除设备信息
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="removeClient"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDeviceInfo(string[] DeviceIDs, bool removeClient = true)
        {
            if (DeviceIDs == null || DeviceIDs.Length == 0)
            {
                return true;
            }
            var dbContext = dbContextPool.Get();
            try
            {
                string str = "";
                foreach (var DeviceID in DeviceIDs)
                {
                    if (removeClient)
                        sipServer.RemoveClient(DeviceID, false);
                    str += $",'{DeviceID}'";
                }
                str = str[1..];
                return await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_Catalog WHERE DeviceID in ({str});DELETE FROM T_DeviceInfo WHERE DeviceID in ({str});") > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }

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
        internal async Task<bool> SaveDeviceInfo(TDeviceInfo deviceInfo)
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
            //        DeviceId = deviceInfo.DeviceID,
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
        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDeviceInfo(TDeviceInfo deviceInfo)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                TDeviceInfo old = await GetDeviceInfo(deviceInfo.DeviceId, dbContext);
                old.NickName = deviceInfo.NickName;
                old.SubscribeExpires = deviceInfo.SubscribeExpires;
                old.UpTime = DateTime.Now;
                dbContext.TDeviceInfos.Update(old);
                return await dbContext.SaveChangesAsync() > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        #endregion


        #region SuperiorInfo
        public async Task<SuperiorInfoEx> GetSuperiorInfo(string id)
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
                    return ToSuperiorInfoEx(lst[0]);
                }
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }
        SuperiorInfoEx ToSuperiorInfoEx(TSuperiorInfo item)
        {
            var si = new SuperiorInfoEx(item);
            si.SetClient(sipServer.Cascade.GetClient(item.Id));
            return si;
        }
        public async Task<DPager<SuperiorInfoEx>> GetSuperiorList()
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var superiors = await dbContext.TSuperiorInfos.ToListAsync();
                List<SuperiorInfoEx> lst = new List<SuperiorInfoEx>();
                foreach (var item in superiors)
                {
                    lst.Add(ToSuperiorInfoEx(item));
                }
                return new DPager<SuperiorInfoEx>(lst, 1, lst.Count, lst.Count);
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
        public async Task<bool> DeleteSuperiors(params string[] ids)
        {
            if (ids.Length == 0)
            {
                return false;
            }
            var dbContext = dbContextPool.Get();
            try
            {
                string str = "";
                foreach (var id in ids)
                {
                    str += $",'{id}'";
                }
                str = str[1..];

                return await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM TSuperiorInfos WHERE ID in ({str}';") > 0;
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }


        #region SuperiorChannel
        public async Task<List<SuperiorChannel>> GetSuperiorChannels(string superiorId)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                var userInfos = from c in dbContext.TSuperiorChannels
                                from t in dbContext.TCatalogs
                                where c.DeviceId == t.DeviceId && c.ChannelId == t.ChannelId && c.SuperiorId == superiorId
                                select new SuperiorChannel(t, c);

                return await userInfos.ToListAsync();
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }

        /// <summary>
        /// 获取通道列表
        /// </summary>
        /// <param name="SuperiorId"></param>
        /// <param name="DeviceID"></param>
        /// <param name="ChannelId"></param>
        /// <param name="Name"></param>
        /// <param name="Parental"></param>
        /// <param name="Manufacturer"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="All"></param>
        /// <returns></returns>
        public async Task<DPager<SuperiorChannel>> GetSuperiorChannels(string SuperiorId, string DeviceID = null, string ChannelId = null, string Name = null, bool? Parental = null, string Manufacturer = null, int Page = 1, int Limit = -1, bool All = false)
        {
            var dbContext = dbContextPool.Get();
            try
            {
                IQueryable<SuperiorChannel> data;
                if (All)
                {
                    //LEFT JOIN
                    data = from catalog in dbContext.TCatalogs
                           join channel in dbContext.TSuperiorChannels on new { catalog.ChannelId, catalog.DeviceId } equals new { channel.ChannelId, channel.DeviceId } into temp
                           from tt in temp.DefaultIfEmpty()
                           select new SuperiorChannel(catalog, tt);
                }
                else
                {
                    data = from channel in dbContext.TSuperiorChannels
                           from catalog in dbContext.TCatalogs
                           where channel.DeviceId == catalog.DeviceId && channel.ChannelId == catalog.ChannelId && channel.SuperiorId == SuperiorId
                           select new SuperiorChannel(catalog, channel);
                }

                if (!string.IsNullOrWhiteSpace(DeviceID))
                {
                    data = data.Where(p => p.DeviceId.Contains(DeviceID));
                }

                if (!string.IsNullOrWhiteSpace(ChannelId))
                {
                    data = data.Where(p => p.ChannelId.Contains(ChannelId));
                }
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    data = data.Where(p => p.Name.Contains(Name));
                }
                if (!string.IsNullOrWhiteSpace(Manufacturer))
                {
                    data = data.Where(p => p.Manufacturer.Contains(Manufacturer));
                }
                if (Parental.HasValue)
                {
                    data = data.Where(p => p.Parental == Parental.Value);
                }
                var sumData = data;
                if (Page > 1)
                {
                    data = data.Skip(GetStart(Page, Limit));
                }
                if (Limit >= 0)
                {
                    data = data.Take(Limit);
                }
                List<SuperiorChannel> lstC = await data.ToListAsync();

                var tuple = await GetSum(Page, Limit, lstC.Count, sumData);

                return new DPager<SuperiorChannel>(lstC, Page, tuple.Item2, tuple.Item1);
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
        }

        /// <summary>
        /// 绑定通道
        /// </summary>
        /// <param name="SuperiorId"></param>
        /// <param name="Add"></param>
        /// <param name="Remove"></param>
        /// <returns></returns>
        public async Task<bool> BindChannels(string SuperiorId, List<TSuperiorChannel> Add, List<TSuperiorChannel> Remove)
        {
            var dbContext = dbContextPool.Get();

            try
            {
                if (Remove == null)
                {
                    Remove = new List<TSuperiorChannel>();
                }
                if (Add == null)
                {
                    Add = new List<TSuperiorChannel>();
                }
                bool flag = false;
                if (Remove.Count > 0)
                {
                    string sql = "";
                    foreach (var item in Remove)
                    {
                        sql += $"DELETE FROM T_SuperiorChannel WHERE SuperiorID='{SuperiorId}' AND CustomChannelID='{item.CustomChannelId}';";
                    }
                    await dbContext.Database.ExecuteSqlRawAsync(sql);
                    flag = true;
                }
                if (Add.Count > 0)
                {
                    foreach (var item in Add)
                    {
                        item.SuperiorId = SuperiorId;
                        if (item.CustomChannelId == null)
                        {
                            item.CustomChannelId = item.ChannelId;
                        }
                    }
                    await dbContext.TSuperiorChannels.AddRangeAsync(Add);
                    flag = true;
                }
                if (flag)
                {
                    await dbContext.SaveChangesAsync();
                    var cl = sipServer.Cascade.GetClient(SuperiorId);
                    if (cl != null)
                    {
                        foreach (var item in Remove)
                            cl.RemoveChannel(item.CustomChannelId);
                        foreach (var item in Add)
                        {
                            var catalog = await GetCatalog(dbContext, item.DeviceId, item.ChannelId);
                            if (catalog != null)
                                cl.AddChannel(new SuperiorChannel(catalog, item));
                        }

                    }
                    return true;
                }
            }
            finally
            {
                dbContextPool.Return(dbContext);
            }
            return false;
        }
        #endregion
        #endregion
    }
}
