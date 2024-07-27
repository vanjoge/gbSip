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
        private class GetGbsContext : IDisposable
        {
            AllNewObjectPool objectPool;
            gbsContext context;
            bool flag;
            public GetGbsContext(AllNewObjectPool objectPool, gbsContext context)
            {
                this.objectPool = objectPool;
                this.context = context;
            }
            public gbsContext Get()
            {
                if (context == null)
                {
                    context = objectPool.Get();
                    flag = true;
                }
                return context;
            }

            public void Dispose()
            {
                if (flag && context != null)
                {
                    var tmp = context;
                    context = null;
                    objectPool.Return(tmp);
                }
                flag = false;
            }
        }
        private class AllNewObjectPool
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
        /// <summary>
        /// 需要搭配using一起用
        /// </summary>
        /// <returns></returns>
        GetGbsContext GetNewCtxManager(gbsContext context = null)
        {
            return new GetGbsContext(dbContextPool, context);
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
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();


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
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

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
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

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

            using (var manager = GetNewCtxManager(dbContext))
            {
                var lst = await manager.Get().TDeviceInfos.Where(p => p.DeviceId == DeviceID).Take(1).ToListAsync();
                if (lst.Count == 0)
                {
                    return null;
                }
                else
                {
                    return lst[0];
                }
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
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

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
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

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
        }

        public async Task<bool> AddDeviceInfo(TDeviceInfo deviceInfo)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                deviceInfo.UpTime = DateTime.Now;
                await dbContext.TDeviceInfos.AddAsync(deviceInfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
        }
        /// <summary>
        /// 保存设备信息
        /// </summary>
        /// <param name="deviceInfo"></param>
        internal async Task<bool> SaveDeviceInfo(TDeviceInfo deviceInfo)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                deviceInfo.UpTime = DateTime.Now;
                dbContext.TDeviceInfos.Update(deviceInfo);
                return await dbContext.SaveChangesAsync() > 0;
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
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                TDeviceInfo old = await GetDeviceInfo(deviceInfo.DeviceId, dbContext);
                old.NickName = deviceInfo.NickName;
                old.SubscribeExpires = deviceInfo.SubscribeExpires;
                old.UpTime = DateTime.Now;
                dbContext.TDeviceInfos.Update(old);
                return await dbContext.SaveChangesAsync() > 0;
            }
        }
        #endregion


        #region SuperiorInfo
        public async Task<SuperiorInfoEx> GetSuperiorInfo(string id)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

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
        }
        SuperiorInfoEx ToSuperiorInfoEx(TSuperiorInfo item)
        {
            var si = new SuperiorInfoEx(item);
            si.SetClient(sipServer.Cascade.GetClient(item.Id));
            return si;
        }
        public async Task<DPager<SuperiorInfoEx>> GetSuperiorList()
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                var superiors = await dbContext.TSuperiorInfos.ToListAsync();
                List<SuperiorInfoEx> lst = new List<SuperiorInfoEx>();
                foreach (var item in superiors)
                {
                    lst.Add(ToSuperiorInfoEx(item));
                }
                return new DPager<SuperiorInfoEx>(lst, 1, lst.Count, lst.Count);
            }
        }
        public async Task<bool> AddSuperior(TSuperiorInfo sinfo)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                await dbContext.TSuperiorInfos.AddAsync(sinfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
        }
        public async Task<bool> UpdateSuperior(TSuperiorInfo sinfo)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                dbContext.TSuperiorInfos.Update(sinfo);
                return await dbContext.SaveChangesAsync() > 0;
            }
        }
        public async Task<bool> DeleteSuperiors(params string[] ids)
        {
            if (ids.Length == 0)
            {
                return false;
            }
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                string str = "";
                foreach (var id in ids)
                {
                    str += $",'{id}'";
                }
                str = str[1..];

                return await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_SuperiorInfo WHERE ID in ({str});") > 0;
            }
        }


        #region SuperiorChannel
        public async Task<List<TGroup>> GetSuperiorGroups(string superiorId)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                var tmp = await dbContext.TSuperiorGroups.Where(p => p.SuperiorId == superiorId).ToListAsync();
                List<string> lst = new List<string>();
                List<string> lstHasChild = new List<string>();
                foreach (var item in tmp)
                {
                    if (item.HasChild)
                    {
                        lstHasChild.Add(item.GroupId);
                    }
                    else
                    {
                        lst.Add(item.GroupId);
                    }
                }
                var groups = from g in dbContext.TGroups where lst.Contains(g.GroupId) select g;
                foreach (var id in lstHasChild)
                {
                    groups = groups.Union(from g in dbContext.TGroups where g.Path.Contains(id) select g);
                }

                //var groups = from c in dbContext.TSuperiorGroups
                //             join
                //               g in dbContext.TGroups on new
                //               {
                //                   GroupId = c.GroupId,
                //                   SuperiorId = c.SuperiorId,
                //                   HasChild = c.HasChild
                //               } equals new
                //               {
                //                   GroupId = g.GroupId,
                //                   SuperiorId = superiorId,
                //                   HasChild = false
                //               }
                //             select g;

                return await groups.ToListAsync();
            }
        }
        public async Task<List<SuperiorChannel>> GetSuperiorChannels(string superiorId, List<string> groupIds)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();
                var channels = from t in dbContext.TCatalogs
                               join
                                 g in dbContext.TGroupBinds on new
                                 {
                                     DeviceId = t.DeviceId,
                                     ChannelId = t.ChannelId,
                                 } equals new
                                 {
                                     DeviceId = g.DeviceId,
                                     ChannelId = g.ChannelId,
                                 }
                               where groupIds.Contains(g.GroupId)
                               select new SuperiorChannel(t, superiorId, g.CustomChannelId, g.GroupId);

                return await channels.ToListAsync();
            }
        }


        #endregion
        #endregion

        #region Group
        public Task<List<TGroup>> GetAllGroups()
        {
            using (var manager = GetNewCtxManager())
            {
                return manager.Get().TGroups.ToListAsync();
            }
        }
        public Task<List<TGroup>> GetGroupsByParent(string parentId)
        {
            using (var manager = GetNewCtxManager())
            {
                return GetGroupsByParent(manager.Get(), parentId);
            }
        }
        internal Task<List<TGroup>> GetGroupsByParent(gbsContext dbContext, string parentId)
        {
            return dbContext.TGroups.Where(p => p.ParentId.Equals(parentId)).ToListAsync();
        }
        internal Task<List<TGroup>> GetGroupsByPath(gbsContext dbContext, string groupId)
        {
            return dbContext.TGroups.Where(p => p.Path.Contains(groupId)).ToListAsync();
        }
        internal async Task<TGroup> GetGroup(gbsContext dbContext, string groupId)
        {
            var lst = await dbContext.TGroups.Where(p => p.GroupId == groupId).Take(1).ToListAsync();
            if (lst.Count == 0)
            {
                return null;
            }
            else
            {
                return lst[0];
            }
        }
        /// <summary>
        /// 添加分组 注意按层级添加 找不到上级时会将其设为无上级
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<bool> AddGroup(TGroup group)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();
                //设置查询加速Path
                if (string.IsNullOrEmpty(group.ParentId) || group.ParentId == group.GroupId)
                {
                    group.ParentId = "";
                    group.Path = group.GroupId;
                }
                else
                {
                    var parent = await GetGroup(dbContext, group.ParentId);
                    if (parent == null)
                    {
                        //找不到上级 设为根节点
                        group.ParentId = "";
                        group.Path = group.GroupId;
                    }
                    else
                    {
                        group.Path = parent.Path + "/" + group.GroupId;
                    }
                }
                await dbContext.TGroups.AddAsync(group);
                return await dbContext.SaveChangesAsync() > 0;
            }
        }
        public async Task<bool> UpdateGroup(TGroup group)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();
                var parent = await GetGroup(dbContext, group.ParentId);
                group.Path = parent.Path + "/" + group.GroupId;
                dbContext.TGroups.Update(group);
                //TODO:未处理包含下级的情况
                return await dbContext.SaveChangesAsync() > 0;
            }
        }
        public async Task<bool> DeleteGroup(string groupId)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();
                if (await dbContext.TGroups.Where(p => p.ParentId.Equals(groupId)).CountAsync() > 0)
                {
                    return false;
                }
                else
                {
                    return await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_Group WHERE GroupId ='{groupId}';DELETE FROM T_SuperiorGroup WHERE GroupId ='{groupId}';DELETE FROM T_GroupBind WHERE GroupId ='{groupId}';") > 0;
                }
            }
        }

        #region

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
        public async Task<DPager<GroupChannel>> GetGroupChannels(string GroupId, string DeviceId = null, string ChannelId = null, string Name = null, bool? Parental = null, string Manufacturer = null, int Page = 1, int Limit = -1, bool All = false)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                IQueryable<GroupChannel> data;

                IQueryable<TCatalog> catalogs = dbContext.TCatalogs;
                if (!string.IsNullOrWhiteSpace(DeviceId))
                {
                    catalogs = catalogs.Where(p => p.DeviceId.Contains(DeviceId));
                }

                if (!string.IsNullOrWhiteSpace(ChannelId))
                {
                    catalogs = catalogs.Where(p => p.ChannelId.Contains(ChannelId));
                }
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    catalogs = catalogs.Where(p => p.Name.Contains(Name));
                }
                if (!string.IsNullOrWhiteSpace(Manufacturer))
                {
                    catalogs = catalogs.Where(p => p.Manufacturer.Contains(Manufacturer));
                }
                if (Parental.HasValue)
                {
                    catalogs = catalogs.Where(p => p.Parental == Parental.Value);
                }

                if (All)
                {
                    //LEFT JOIN
                    data = from catalog in catalogs
                           join channel in dbContext.TGroupBinds.Where(p => p.GroupId == GroupId) on new { catalog.ChannelId, catalog.DeviceId } equals new { channel.ChannelId, channel.DeviceId } into temp
                           from tt in temp.DefaultIfEmpty()
                           select new GroupChannel(catalog, tt);
                }
                else
                {
                    data = from catalog in catalogs
                           join channel in dbContext.TGroupBinds.Where(p => p.GroupId == GroupId) on new { catalog.ChannelId, catalog.DeviceId } equals new { channel.ChannelId, channel.DeviceId }
                           select new GroupChannel(catalog, channel);
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
                var lstC = await data.ToListAsync();

                var tuple = await GetSum(Page, Limit, lstC.Count, sumData);

                return new DPager<GroupChannel>(lstC, Page, tuple.Item2, tuple.Item1);
            }
        }
        /// <summary>
        /// 绑定通道
        /// </summary>
        /// <param name="SuperiorId"></param>
        /// <param name="Add"></param>
        /// <param name="Remove"></param>
        /// <returns></returns>
        public async Task<bool> BindChannels(string GroupId, List<TGroupBind> Add, List<TGroupBind> Remove)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();

                if (Remove == null)
                {
                    Remove = new List<TGroupBind>();
                }
                if (Add == null)
                {
                    Add = new List<TGroupBind>();
                }
                bool flag = false;
                if (Remove.Count > 0)
                {
                    string sql = "";
                    foreach (var item in Remove)
                    {
                        sql += $"DELETE FROM T_GroupBind WHERE GroupID='{GroupId}' AND CustomChannelID='{item.CustomChannelId}';";
                    }
                    await dbContext.Database.ExecuteSqlRawAsync(sql);
                    flag = true;
                }
                if (Add.Count > 0)
                {
                    foreach (var item in Add)
                    {
                        item.GroupId = GroupId;
                        if (item.CustomChannelId == null)
                        {
                            item.CustomChannelId = item.ChannelId;
                        }
                    }
                    await dbContext.TGroupBinds.AddRangeAsync(Add);
                    flag = true;
                }
                if (flag)
                {
                    await dbContext.SaveChangesAsync();
                    //var cl = sipServer.Cascade.GetClient(GroupId);
                    //if (cl != null)
                    //{
                    //    foreach (var item in Remove)
                    //        cl.RemoveChannel(item.CustomChannelId);
                    //    foreach (var item in Add)
                    //    {
                    //        var catalog = await GetCatalog(dbContext, item.DeviceId, item.ChannelId);
                    //        if (catalog != null)
                    //            cl.AddChannel(new SuperiorChannel(catalog, item));
                    //    }

                    //}
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 绑定上级平台
        /// </summary>
        /// <param name="SuperiorId"></param>
        /// <param name="GroupId"></param>
        /// <param name="HasChild"></param>
        /// <param name="Cancel"></param>
        /// <returns></returns>
        public async Task<bool> BindSuperior(string SuperiorId, string GroupId, bool HasChild, bool Cancel)
        {
            using (var manager = GetNewCtxManager())
            {
                var dbContext = manager.Get();
                List<TGroup> groups;
                if (HasChild)
                {
                    groups = await GetGroupsByPath(dbContext, GroupId);
                }
                else
                {
                    groups = new List<TGroup>
                    {
                        await GetGroup(dbContext,GroupId)
                    };
                }
                //删除原有的
                string str = "";
                foreach (var item in groups)
                {
                    str += $",'{item.GroupId}'";
                }
                str = str[1..];
                await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM T_SuperiorGroup WHERE GroupID in ({str}) AND SuperiorID='{SuperiorId}';");
                if (!Cancel)
                {
                    await dbContext.TSuperiorGroups.AddAsync(new TSuperiorGroup
                    {
                        GroupId = GroupId,
                        HasChild = HasChild,
                        SuperiorId = SuperiorId,
                    });
                    return await dbContext.SaveChangesAsync() > 0;
                }
            }
            return true;
        }
        #endregion

        #endregion
    }
}