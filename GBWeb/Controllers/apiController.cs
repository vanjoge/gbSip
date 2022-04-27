using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SQ.Base;

namespace GBWeb.Controllers
{
    public class apiController : ControllerBase
    {

        public async Task<string> VideoControl(string Content, bool IsSuperiorPlatformSend = false)
        {
            SQ.Base.Log.WriteLog4("VideoControl接口收到数据 Content=" + Content + (IsSuperiorPlatformSend ? "&IsSuperiorPlatformSend=true" : ""));
            if (string.IsNullOrWhiteSpace(Content))
            {
                return "-1";
            }
            return await Program.sipServer.HandleJT1078(Content, IsSuperiorPlatformSend);
        }
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
