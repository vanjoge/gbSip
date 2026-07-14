using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        /// 获取验证码图片
        /// </summary>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        public async Task<ApiResult<CaptchaResult>> Captcha(int width = 120, int height = 40)
        {
            // 从Cookie获取或生成SessionID
            string sessionId;
            if (!Request.Cookies.TryGetValue("captcha_session", out sessionId) || string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString("N");
                Response.Cookies.Append("captcha_session", sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    MaxAge = TimeSpan.FromMinutes(30),
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });
            }

            // 生成随机验证码（数字+字母混合，排除容易混淆的字符）
            var random = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var code = "";
            for (int i = 0; i < 4; i++)
            {
                code += chars[random.Next(chars.Length)];
            }

            // 使用SessionID作为key，每个用户只有一个验证码
            await Program.sipServer.Cache.SetAsync($"captcha:{sessionId}", code.ToLower(), 5);

            // 生成验证码图片
            using (var bitmap = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // 背景色
                    graphics.Clear(Color.White);

                    // 绘制干扰线
                    for (int i = 0; i < 15; i++)
                    {
                        var pen = new Pen(Color.FromArgb(random.Next(100, 200), random.Next(100, 200), random.Next(100, 200)));
                        var x1 = random.Next(width);
                        var y1 = random.Next(height);
                        var x2 = random.Next(width);
                        var y2 = random.Next(height);
                        graphics.DrawLine(pen, x1, y1, x2, y2);
                    }

                    // 绘制验证码文字（每个字符随机角度旋转）
                    var font = new Font("Arial", 18, FontStyle.Bold);
                    var brush = new SolidBrush(Color.FromArgb(random.Next(50, 150), random.Next(50, 150), random.Next(50, 150)));

                    float startX = 15;
                    for (int i = 0; i < code.Length; i++)
                    {
                        // 随机角度旋转（-30度到30度）
                        float angle = random.Next(-30, 30);

                        // 创建旋转后的文字
                        var charBitmap = new Bitmap(30, 30);
                        using (var charGraphics = Graphics.FromImage(charBitmap))
                        {
                            charGraphics.Clear(Color.Transparent);
                            charGraphics.TranslateTransform(15, 15);
                            charGraphics.RotateTransform(angle);
                            charGraphics.DrawString(code[i].ToString(), font, brush, -10, -10);
                            charGraphics.ResetTransform();
                        }

                        // 绘制到主画布
                        graphics.DrawImage(charBitmap, startX, 5);
                        startX += 25;
                    }

                    // 添加噪点
                    for (int i = 0; i < 100; i++)
                    {
                        bitmap.SetPixel(random.Next(width), random.Next(height),
                            Color.FromArgb(random.Next(100, 200), random.Next(100, 200), random.Next(100, 200)));
                    }

                    // 绘制干扰曲线
                    for (int i = 0; i < 3; i++)
                    {
                        var pen = new Pen(Color.FromArgb(random.Next(100, 200), random.Next(100, 200), random.Next(100, 200)), 1);
                        var points = new System.Drawing.Point[5];
                        for (int j = 0; j < 5; j++)
                        {
                            points[j] = new System.Drawing.Point(random.Next(width), random.Next(height));
                        }
                        graphics.DrawCurve(pen, points);
                    }
                }

                // 转换为Base64
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    var img = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    // 返回SessionID作为ID
                    return RetApiResult(new CaptchaResult { Id = sessionId, Img = img });
                }
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="Password">密码（MD5加密后）</param>
        /// <param name="captchaId">验证码ID</param>
        /// <param name="captcha">验证码</param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public async Task<ApiResult<LoginResult>> Login(string UserName, string Password, string captchaId, string captcha)
        {
            // 强制验证码校验
            if (string.IsNullOrEmpty(captchaId) || string.IsNullOrEmpty(captcha))
            {
                return new ApiResult<LoginResult> { code = 422, message = "请输入验证码" };
            }

            var storedCode = await Program.sipServer.Cache.GetAsync($"captcha:{captchaId}");
            if (string.IsNullOrEmpty(storedCode))
            {
                return new ApiResult<LoginResult> { code = 422, message = "验证码已过期，请重新获取" };
            }

            if (!storedCode.Equals(captcha.ToLower()))
            {
                return new ApiResult<LoginResult> { code = 422, message = "验证码错误" };
            }

            // 验证成功后立即删除验证码（防止重复使用）
            await Program.sipServer.Cache.DeleteAsync($"captcha:{captchaId}");

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
