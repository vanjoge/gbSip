using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.XML;
using GBWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SipServer;
using SipServer.DBModel;
using SipServer.Models;
using SQ.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace GBWeb.Controllers
{
    /// <summary>
    /// 通道接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChannelController : BaseApi
    {
        /// <summary>
        /// 获取设备通道信息
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <param name="ParentId"></param>
        /// <param name="ChannelId"></param>
        /// <param name="Name"></param>
        /// <param name="Parental"></param>
        /// <param name="Manufacturer"></param>
        /// <param name="Online"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<DPager<Channel>>> GetChannelList(string DeviceId, string ParentId, string ChannelId, string Name, bool? Parental, string Manufacturer, bool? Online, int Page = 1, int Limit = 10)
        {
            return await RetApiResult(Program.sipServer.DB.GetChannelList(DeviceId, ParentId, ChannelId, Name, Parental, Manufacturer, Online, Page, Limit));
        }
        public class UpdateChannelModel : ChannelConf
        {
            public string DeviceId { get; set; }
            public string ChannelId { get; set; }
        }
        /// <summary>
        /// 更新通道信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateChannel(UpdateChannelModel info)
        {
            return await RetApiResult(Program.sipServer.DB.UpdateChannelConf(info.DeviceId, info.ChannelId, info));
        }
        /// <summary>
        /// 
        /// </summary>
        public class DeleteChannelModel
        {
            public string DeviceId { get; set; }
            public string[] ChannelIds { get; set; }
        }
        /// <summary>
        /// 删除通道
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> DeleteChannel(DeleteChannelModel model)
        {
            if (model == null)
                return RetApiResult(false);
            return RetApiResult(await Program.sipServer.DB.DeleteChannel(model.DeviceId, model.ChannelIds));
        }

    }
}
