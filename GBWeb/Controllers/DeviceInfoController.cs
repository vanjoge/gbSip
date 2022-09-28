using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.XML;
using GBWeb.Attribute;
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
    /// 设备接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeviceInfoController : BaseApi
    {
        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <param name="DeviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<TDeviceInfo>> GetDeviceInfo([FromCustom] string DeviceId)
        {
            return RetApiResult(await Program.sipServer.DB.GetDeviceInfo(DeviceId));
        }
        /// <summary>
        /// 获取设备列表 支持模糊查询
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <param name="DeviceName"></param>
        /// <param name="Manufacturer"></param>
        /// <param name="Online"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<DPager<TDeviceInfo>>> GetDeviceList(string DeviceId, string DeviceName, string Manufacturer, bool? Online, int Page = 1, int Limit = 10)
        {
            return await RetApiResult(Program.sipServer.DB.GetDeviceList(DeviceId, DeviceName, Manufacturer, Online, Page, Limit));
        }
        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateDevice(TDeviceInfo info)
        {
            return await RetApiResult(Program.sipServer.DB.UpdateDeviceInfo(info));
        }
        /// <summary>
        /// 
        /// </summary>
        public class DeleteDeviceModel
        {
            public string[] DeviceIds { get; set; }
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> DeleteDevice(DeleteDeviceModel model)
        {
            if (model == null)
                return RetApiResult(false);
            return RetApiResult(await Program.sipServer.DB.DeleteDeviceInfo(model.DeviceIds));
        }


        /// <summary>
        /// 开始刷新通道
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> SendRefreshChannel(string DeviceId)
        {
            if (Program.sipServer.TryGetClient(DeviceId, out var client))
            {
                await client.RefreshChannel();
                return RetApiResult(true);
            }
            return RetApiResult(false);
        }
    }
}
