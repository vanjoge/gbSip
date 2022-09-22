using GB28181.XML;
using GBWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SipServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SQ.Base;
using SipServer.Models;
using Microsoft.AspNetCore.Http;

namespace GBWeb.Controllers
{
    public class DeviceController : Controller
    {

        public DeviceController()
        {
        }

        public async Task<IActionResult> Index(string DeviceId, string DeviceName, string Manufacturer, bool? Online = true, int Page = 1, int Limit = -1)
        {
            var lstDev = await Program.sipServer.DB.GetDeviceList(DeviceId, DeviceName, Manufacturer, Online, Page, Limit);
            return View(lstDev);
        }
        public async Task<IActionResult> Channels(string DeviceId)
        {
            var model = await Program.sipServer.DB.GetChannelList(DeviceId);
            return View(model);
        }

        public async Task<IActionResult> RefreshChannel(string DeviceId)
        {
            if (Program.sipServer.TryGetClient(DeviceId, out var client))
            {
                await client.RefreshChannel();
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(string DeviceId)
        {
            return View(await Program.sipServer.DB.GetDeviceInfo(DeviceId));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string DeviceId, IFormCollection collection)
        {
            await Program.sipServer.DB.DeleteDeviceInfo(new string[] { DeviceId });
            return RedirectToAction("Index");
        }
    }
}
