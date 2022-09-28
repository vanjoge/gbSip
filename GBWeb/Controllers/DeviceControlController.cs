using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GB28181.PTZ;
using GB28181.XML;
using GBWeb.Attribute;
using GBWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SipServer;
using SQ.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace GBWeb.Controllers
{
    /// <summary>
    /// 设备控制接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeviceControlController : BaseApi
    {
        /// <summary>
        /// PTZ指令
        /// </summary>
        /// <param name="DeviceId">设备ID</param>
        /// <param name="Channel">通道ID PTZXML中的DeviceID</param>
        /// <param name="Address">地址 0-4095，其中0作为广播地址</param>
        /// <param name="ZoomIn">放大速度 0-15 非必须 不传表示停止</param>
        /// <param name="ZoomOut">缩小速度 0-15 非必须 不传表示停止</param>
        /// <param name="Up">上移速度 0-255 非必须 不传表示停止</param>
        /// <param name="Down">下移速度 0-255 非必须 不传表示停止</param>
        /// <param name="Left">左转速度 0-255 非必须 不传表示停止</param>
        /// <param name="Right">右转速度 0-255 非必须 不传表示停止</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<ApiResult<bool>> PTZCtrl([FromCustom] string DeviceId, [FromCustom] string Channel, [FromCustom] ushort Address, [FromCustom] byte? ZoomIn, [FromCustom] byte? ZoomOut, [FromCustom] byte? Up, [FromCustom] byte? Down, [FromCustom] byte? Left, [FromCustom] byte? Right)
        {
            if (Program.sipServer.TryGetClient(DeviceId, out var client))
            {
                var cmd = new PTZCmd
                {
                    Address = Address,
                    ZoomIn = ZoomIn,
                    ZoomOut = ZoomOut,
                    Up = Up,
                    Down = Down,
                    Left = Left,
                    Right = Right
                };
                var control = new DeviceControl
                {
                    DeviceID = Channel,
                    PTZCmd = cmd.ToPTZStr(),
                };
                await client.Send_DeviceControl(control);
                return RetApiResult(true);
            }
            return RetApiResult(false);
        }
    }
}
