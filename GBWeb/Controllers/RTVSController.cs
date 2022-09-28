using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.MANSRTSP;
using GB28181.XML;
using GBWeb.Attribute;
using Microsoft.AspNetCore.Authorization;
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
    [ApiController, AuthApi]
    public class RTVSController : ControllerBase
    {
        private string Base64ToStr(string base64)
        {
            var buff = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(buff);
        }
        /// <summary>
        /// 28181发起音视频请求
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="SDP">SDP信息，BASE64编码</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <param name="TalkCov">对讲转换策略 0 发现设备不支持时返回2 1 不检查原样发送 2 发现设备不支持时直接转换为广播发送</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Invite(string DeviceID, string Channel, string InviteID, string SDP, string CTags, SipServer.Models.InviteTalk TalkCov = SipServer.Models.InviteTalk.Auto)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceID, out var client))
                {
                    return await client.Send_INVITE(Channel, InviteID, Base64ToStr(SDP), TalkCov);
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
        }


        /// <summary>
        /// 28181发起广播
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="SourceID">语音输入设备的设备编码 为null取Channel</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Broadcast(string DeviceID, string Channel, string SourceID, string InviteID, string CTags)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceID, out var client))
                {
                    await client.Send_Broadcast(SourceID, Channel, InviteID);
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
        }
        /// <summary>
        /// 28181关闭音视频请求
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Bye(string DeviceID, string Channel, string InviteID, string CTags)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceID, out var client))
                {
                    await client.Send_Bye(InviteID);
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
        }

        /// <summary>
        /// 发送MANSRTSP
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <param name="MANSRTSP">MANSRTSP，BASE64编码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> MANSRTSP(string DeviceID, string Channel, string InviteID, string CTags, string MANSRTSP)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceID, out var client))
                {
                    await client.Send_MANSRTSP(InviteID, Base64ToStr(MANSRTSP));
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
        }

        /// <summary>
        /// 获取设备录像
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <param name="OrderID"></param>
        /// <param name="CTags"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> GetRecordInfo(string DeviceID, string OrderID, string CTags, RecordInfoQuery query)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceID, out var client))
                {
                    await client.Send_GetRecordInfo(OrderID, query);
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
        }
    }
}
