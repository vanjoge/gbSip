using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.XML;
using GBWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SipServer;
using SQ.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace GBWeb.Controllers
{
    /// <summary>
    /// 登录相关接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : BaseApi
    {
        /// <summary>
        /// 登录参数
        /// </summary>
        public class LoginParams
        {
            /// <summary>
            /// 用户名
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 密码
            /// </summary>
            public string Password { get; set; }
        }

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

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginParams">登录参数</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<LoginResult>> Login(LoginParams loginParams)
        {
            return RetApiResult(new LoginResult { Token = SIPSorcery.SIP.CallProperties.CreateNewTag() });
        }


        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, ApiAuthorize]
        public async Task<ApiResult<UserInfo>> Info()
        {
            return RetApiResult(new UserInfo
            {
                Name = "admin"
            });
        }
        /// <summary>
        /// 获取权限
        /// </summary>
        /// <returns></returns>
        [HttpGet, ApiAuthorize]
        public async Task<ApiResult<PermMenu>> Permmenu()
        {
            List<Menu> menus = new List<Menu>();
            List<string> perms = new List<string>();
            menus.Add(new Menu
            {
                Id = 1,
                Name = "接口",
                Router = "/swagger/index.html",
                Type = 0,
                Icon = "icon-shezhi",

                OrderNum = 255,
                Keepalive = false,
                IsShow = true

            });
            return RetApiResult(new PermMenu
            {
                Menus = menus,
                Perms = perms
            });
        }
        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>

        [HttpPost, ApiAuthorize]
        public async Task<ApiResult> Logout()
        {
            return new ApiResult(200);
        }
    }
}
