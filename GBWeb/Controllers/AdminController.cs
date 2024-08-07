﻿using System;
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
using SipServer.Models;
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
        /// 登录
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public async Task<ApiResult<LoginResult>> Login(string UserName, string Password)
        {
            var ret = await Program.sipServer.DB.Login(UserName, Password);
            if (ret != null)
            {
                return RetApiResult(ret);
            }
            else
            {
                return new ApiResult<LoginResult> { code = 422, message = "用户名或密码错误" };
            }
        }


        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult<UserInfo> Info()
        {
            return RetApiResult(new UserInfo
            {
                Name = Program.sipServer.Settings.WebUsrName
            });
        }
        /// <summary>
        /// 获取权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult<PermMenu> Permmenu()
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
            perms.Add("DeviceInfo:UpdateDevice");
            perms.Add("DeviceInfo:DeleteDevice");
            perms.Add("Superior:CreateSuperior");
            perms.Add("Superior:UpdateSuperior");
            perms.Add("Superior:DeleteSuperiors");
            perms.Add("Group:CreateGroup");
            perms.Add("Group:UpdateGroup");
            perms.Add("Group:DeleteGroups");
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

        [HttpPost]
        public async Task<ApiResult> Logout()
        {
            await Program.sipServer.DB.Logout(GetAuthorization());
            return new ApiResult(200);
        }
    }
}
