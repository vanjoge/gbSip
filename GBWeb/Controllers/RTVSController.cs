using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SQ.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace GBWeb.Controllers
{
    /// <summary>
    /// RTVS接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RTVSController : ControllerBase
    {
        /// <summary>
        /// 808指令接口
        /// </summary>
        /// <param name="Content">808协议16进制字符串(包头+包体) 包头不含7E、未转义、流水号需要808平台替换</param>
        /// <param name="IsSuperiorPlatformSend">是否是上级平台发送，网关可用此字段确定是否由上级平台发起。一般为true才会包含此字段，为false时此字段不传</param>
        [HttpGet]
        public async Task<string> VideoControl(string Content, bool IsSuperiorPlatformSend = false)
        {
            SQ.Base.Log.WriteLog4("VideoControl接口收到数据 Content=" + Content + (IsSuperiorPlatformSend ? "&IsSuperiorPlatformSend=true" : ""));
            if (string.IsNullOrWhiteSpace(Content))
            {
                return "-1";
            }
            return await Program.sipServer.HandleJT1078(Content, IsSuperiorPlatformSend);
        }
        /// <summary>
        /// 0x9105实时音视频传输状态通知
        /// </summary>
        /// <param name="Content">JT0x9105SimItem 数组的JSON</param>
        [HttpPost]
        public async Task<string> WCF0x9105(string Content)
        {
            SQ.Base.Log.WriteLog4("WCF0x9105接口收到数据 Content=" + Content);
            if (string.IsNullOrWhiteSpace(Content))
            {
                return "-1";
            }
            return await Program.sipServer.HandleJT1078_0x9105(Content);
        }
    }
}
