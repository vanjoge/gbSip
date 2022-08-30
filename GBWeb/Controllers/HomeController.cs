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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string DeviceId, string DeviceName, string Manufacturer, bool? Online = true, int Page = 1, int Limit = -1)
        {
            var lstDev = await Program.sipServer.DB.GetDeviceList(DeviceId, DeviceName, Manufacturer, Online, Page, Limit);
            return View(lstDev);
        }
        public async Task<IActionResult> Channels(string DeviceID)
        {
            var model = await Program.sipServer.DB.GetChannelList(DeviceID);
            return View(model);
        }

        public async Task<IActionResult> RefreshChannel(string DeviceID)
        {
            if (Program.sipServer.TryGetClient(DeviceID, out var client))
            {
                await client.RefreshChannel();
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(string DeviceID)
        {
            return View(await Program.sipServer.DB.GetDeviceInfo(DeviceID));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string DeviceID, IFormCollection collection)
        {
            await Program.sipServer.DB.DeleteDeviceInfo(new string[] { DeviceID });
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
