using System;
using System.Collections.Generic;
using GB28181.XML;
using SipServer.DBModel;
using SQ.Base;

namespace SipServer.Models
{
    /// <summary>
    /// 验证码结果
    /// </summary>
    public class CaptchaResult
    {
        /// <summary>
        /// 验证码ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 验证码图片(Base64)
        /// </summary>
        public string Img { get; set; }
    }
}