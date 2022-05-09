using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.XML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SipServer;
using SQ.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace GBWeb.Controllers
{
    /// <summary>
    /// 设备接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeviceInfoController : ControllerBase
    {
        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <param name="DeviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<DeviceInfo> GetDeviceInfo([FromCustom] string DeviceId)
        {
            return await Program.sipServer.RedisHelper.HashGetAsync<DeviceInfo>(RedisConstant.DevInfoHead + DeviceId, RedisConstant.DeviceInfoKey);
        }
    }
}
