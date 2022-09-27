using System;
using System.Collections.Generic;
using GB28181.XML;
using SipServer.DBModel;
using SQ.Base;

namespace SipServer.Models
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Token { get; set; }
    }
}
