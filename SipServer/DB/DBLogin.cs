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
            if (UserName == sipServer.Settings.WebUsrName && Password == sipServer.Settings.WebUsrPwd)
            {
                var ret = new LoginResult { Token = SIPSorcery.SIP.CallProperties.CreateNewTag() };
                await RedisHelper.StringSetAsync(RedisConstant.TokenKey + ret.Token, UserName, TimeSpan.FromDays(1));
                return ret;
            }
            else
            {
                return null;
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
