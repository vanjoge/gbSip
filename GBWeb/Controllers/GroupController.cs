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
    /// 分组管理接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GroupController : BaseApi
    {
        /// <summary>
        /// 获取分组信息
        /// </summary>
        /// <param name="ParentId">上级分组ID，传空或NULL表示查询根节点</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<TGroup>>> GetGroupsByParent([FromCustom] string ParentId)
        {
            return await RetApiResult(Program.sipServer.DB.GetGroupsByParent(ParentId ?? ""));
        }
        /// <summary>
        /// 获取全部分组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<TGroup>>> GetAllGroups()
        {
            return await RetApiResult(Program.sipServer.DB.GetAllGroups());
        }
        /// <summary>
        /// 添加分组
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> CreateGroup(TGroup info)
        {
            return await RetApiResult(Program.sipServer.DB.AddGroup(info));
        }
        /// <summary>
        /// 更新分组
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateGroup(TGroup info)
        {
            return await RetApiResult(Program.sipServer.DB.UpdateGroup(info));
        }
        /// <summary>
        /// 删除分组
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> DeleteGroups(DeleteModelIds model)
        {
            if (model == null || model.Ids.Length != 1)
                return RetApiResult(false);
            return await RetApiResult(Program.sipServer.DB.DeleteGroup(model.Ids[0]));
        }


        /// <summary>
        /// 获取通道信息
        /// </summary>
        /// <param name="SuperiorId"></param>
        /// <param name="DeviceId"></param>
        /// <param name="ParentId"></param>
        /// <param name="ChannelId"></param>
        /// <param name="Name"></param>
        /// <param name="Parental"></param>
        /// <param name="Manufacturer"></param>
        /// <param name="OnlyBind">仅筛选已绑定的</param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<DPager<GroupChannel>>> GetChannelList(string GroupID, string DeviceId, string ChannelId, string Name, bool? Parental, string Manufacturer, bool OnlyBind, int Page = 1, int Limit = 10)
        {
            return await RetApiResult(Program.sipServer.DB.GetGroupChannels(GroupID, DeviceId, ChannelId, Name, Parental, Manufacturer, Page, Limit, !OnlyBind));
        }

        /// <summary>
        /// 
        /// </summary>
        public class BindChannelsModel
        {
            public string GroupID { get; set; }
            public List<TGroupBind> Add { get; set; }
            public List<TGroupBind> Remove { get; set; }
        }
        /// <summary>
        /// 绑定通道
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> BindChannels(BindChannelsModel model)
        {
            return await RetApiResult(Program.sipServer.DB.BindChannels(model.GroupID, model.Add, model.Remove));
        }
        /// <summary>
        /// 
        /// </summary>
        public class BindSuperiorModel
        {
            public string SuperiorId { get; set; }
            public string GroupId { get; set; }
            public bool HasChild { get; set; }
            public bool Cancel { get; set; }
        }
        /// <summary>
        /// 绑定上级平台
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> BindSuperior(BindSuperiorModel model)
        {
            return await RetApiResult(Program.sipServer.DB.BindSuperior(model.SuperiorId, model.GroupId, model.HasChild, model.Cancel));
        }
    }
}
