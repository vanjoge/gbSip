using GBWeb.Attribute;
using GBWeb.Models;
using Microsoft.AspNetCore.Mvc;
using SipServer.DBModel;
using SipServer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GBWeb.Controllers
{
    /// <summary>
    /// 级联上级接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SuperiorController : BaseApi
    {
        /// <summary>
        /// 获取上级信息
        /// </summary>
        /// <param name="Id">上级ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<SuperiorInfoEx>> GetSuperior([FromCustom] string Id)
        {
            return await RetApiResult(Program.sipServer.DB.GetSuperiorInfo(Id));
        }
        /// <summary>
        /// 获取上级列表 支持模糊查询
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <param name="DeviceName"></param>
        /// <param name="Manufacturer"></param>
        /// <param name="Online"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<DPager<SuperiorInfoEx>>> GetSuperiorList(string DeviceId, string DeviceName, string Manufacturer, bool? Online, int Page = 1, int Limit = 10)
        {
            return await RetApiResult(Program.sipServer.DB.GetSuperiorList());
        }
        /// <summary>
        /// 添加上级
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> CreateSuperior(TSuperiorInfo info)
        {
            info.Id = Guid.NewGuid().ToString();
            info.ServerRealm = info.ServerId.Substring(0, 10);
            SuperiorInfoEx.Check(info);
            return await RetApiResult(Program.sipServer.Cascade.Add(info));
        }
        /// <summary>
        /// 更新上级
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateSuperior(TSuperiorInfo info)
        {
            info.ServerRealm = info.ServerId.Substring(0, 10);
            return await RetApiResult(Program.sipServer.Cascade.Update(info));
        }
        /// <summary>
        /// 删除上级
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> DeleteSuperiors(DeleteModelIds model)
        {
            if (model == null)
                return RetApiResult(false);
            return RetApiResult(await Program.sipServer.Cascade.Remove(model.Ids));
        }

        ///// <summary>
        ///// 获取通道信息
        ///// </summary>
        ///// <param name="SuperiorId"></param>
        ///// <param name="DeviceId"></param>
        ///// <param name="ParentId"></param>
        ///// <param name="ChannelId"></param>
        ///// <param name="Name"></param>
        ///// <param name="Parental"></param>
        ///// <param name="Manufacturer"></param>
        ///// <param name="OnlyBind">仅筛选已绑定的</param>
        ///// <param name="Page"></param>
        ///// <param name="Limit"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<ApiResult<DPager<SuperiorChannel>>> GetChannelList(string SuperiorId, string DeviceId, string ParentId, string ChannelId, string Name, bool? Parental, string Manufacturer, bool OnlyBind, int Page = 1, int Limit = 10)
        //{
        //    return await RetApiResult(Program.sipServer.DB.GetSuperiorChannels(SuperiorId, DeviceId, ChannelId, Name, Parental, Manufacturer, Page, Limit, !OnlyBind));
        //}
        /// <summary>
        /// 
        /// </summary>
        public class BindChannelsModel
        {
            public string SuperiorId { get; set; }
            public List<TSuperiorGroup> Add { get; set; }
            public List<TSuperiorGroup> Remove { get; set; }
        }
        ///// <summary>
        ///// 绑定通道
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<ApiResult<bool>> BindChannels(BindChannelsModel model)
        //{
        //    return await RetApiResult(Program.sipServer.DB.BindChannels(model.SuperiorId, model.Add, model.Remove));
        //}
    }
}
