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

        public async Task<IActionResult> Index(bool onlyOnline = true, int start = 0, int count = -1)
        {
            var lstDev = await Program.sipServer.DB.GetDeviceInfoList(onlyOnline, start, count);
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
        public async Task<IActionResult> Delete(string Did)
        {
            return View(await Program.sipServer.DB.GetDeviceInfo(Did));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string Did, IFormCollection collection)
        {
            await Program.sipServer.DB.DeleteDeviceInfo(Did);
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
