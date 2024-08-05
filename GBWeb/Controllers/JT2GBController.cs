using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GB28181.XML;
using GBWeb.Attribute;
using GBWeb.Models;
using Microsoft.AspNetCore.Mvc;
using SipServer.Models.JT;

namespace GBWeb.Controllers
{
    /// <summary>
    /// JT1078转GB28181接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController, AuthApi]
    public class JT2GBController : ControllerBase
    {
        /// <summary>
        /// JT1078车机上线/心跳通知(批量)
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Online(List<JTItem> lst)
        {
            try
            {
                await Program.sipServer.JT2GB.AddJTItems(lst);

                return new ApiResult(200);
            }
            catch
            {
                return new ApiResult(500) { message = "err" };
            }
        }
        /// <summary>
        /// JT1078车机下线通知(批量)
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Offline(List<JTKey> lst)
        {
            try
            {
                await Program.sipServer.JT2GB.RemoveJTItems(lst);

                return new ApiResult(200);
            }
            catch
            {
                return new ApiResult(500) { message = "err" };
            }
        }
    }
}
