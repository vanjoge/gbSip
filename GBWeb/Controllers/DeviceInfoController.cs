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
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<DeviceInfo> GetDeviceInfo([FromCustom] string DeviceID)
        {
            return await Program.sipServer.DB.GetDeviceInfo(DeviceID);
        }
        /// <summary>
        /// 获取所有设备ID
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<List<string>> GetAllDeviceID()
        {
            return await Program.sipServer.DB.GetDeviceIds();
        }
    }
}
