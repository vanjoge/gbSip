using DnsClient.Protocol;
using GB28181.XML;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.ObjectPool;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Utilities.Collections;
using RedisHelp;
using SipServer.DBModel;
using SipServer.Models;
using SQ.Base;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;
using static SIPSorcery.Net.Mjpeg;
using static SIPSorcery.Net.SrtpCipherF8;

namespace SipServer.DB
{
    partial class DBInfo
    {
        public async Task<LoginResult> Login(string UserName, string Password)
        {
            if (UserName == sipServer.Settings.WebUsrName)
            {
                // 强制验证密码必须是32位MD5格式
                if (string.IsNullOrEmpty(Password) || Password.Length != 32 || !IsHexString(Password))
                {
                    return null;
                }

                // 将配置中的明文密码转换为MD5后对比
                string expectedPassword = sipServer.Settings.WebUsrPwd;
                string md5OfExpected = GetMd5Hash(expectedPassword);

                if (Password.Equals(md5OfExpected, StringComparison.OrdinalIgnoreCase))
                {
                    var ret = new LoginResult { Token = SIPSorcery.SIP.CallProperties.CreateNewTag() };
                    await RedisHelper.StringSetAsync(RedisConstant.TokenKey + ret.Token, UserName, TimeSpan.FromDays(1));
                    return ret;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断字符串是否为十六进制字符串
        /// </summary>
        private bool IsHexString(string str)
        {
            foreach (char c in str)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 获取字符串的MD5哈希值
        /// </summary>
        private string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public async Task<bool> CheckToken(string Token)
        {
            return await RedisHelper.StringGetAsync(RedisConstant.TokenKey + Token) != null;
        }
        public async Task<bool> Logout(string Token)
        {
            return await RedisHelper.GetDatabase().KeyDeleteAsync(RedisConstant.TokenKey + Token);
        }
    }
}
