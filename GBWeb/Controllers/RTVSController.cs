using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.MANSRTSP;
using GB28181.XML;
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
        private string Base64ToStr(string base64)
        {
            var buff = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(buff);
        }
        /// <summary>
        /// 28181发起音视频请求
        /// </summary>
        /// <param name="DeviceId">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="SDP">SDP信息，BASE64编码</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> INVITE(string DeviceId, string Channel, string InviteID, string SDP, string CTags)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceId, out var client))
                {
                    await client.Send_INVITE(Channel, InviteID, Base64ToStr(SDP));
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
        /// <param name="DeviceId">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> BYE(string DeviceId, string Channel, string InviteID, string CTags)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceId, out var client))
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
        /// <param name="DeviceId">设备ID</param>
        /// <param name="Channel">通道ID，一般是IPC的ID</param>
        /// <param name="InviteID">请求ID，用以区分多次请求，可用做SIP的FromTag</param>
        /// <param name="CTags">RTVS CTags</param>
        /// <param name="MANSRTSP">MANSRTSP，BASE64编码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> MANSRTSP(string DeviceId, string Channel, string InviteID, string CTags, string MANSRTSP)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceId, out var client))
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
        /// <param name="DeviceId"></param>
        /// <param name="OrderId"></param>
        /// <param name="CTags"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> GetRecordInfo(string DeviceId, string OrderId, string CTags, RecordInfoQuery query)
        {
            try
            {
                SQ.Base.Log.WriteLog4(this.HttpContext.Request.Path + this.HttpContext.Request.QueryString);
                if (Program.sipServer.TryGetClient(DeviceId, out var client))
                {
                    await client.Send_GetRecordInfo(OrderId, query);
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
