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

        public async Task<IActionResult> Index(bool onlyOnline = true, long start = 0, long end = -1)
        {
            List<DeviceInfoEx> lstDev;
            if (onlyOnline)
            {
                lstDev = new List<DeviceInfoEx>();
                Program.sipServer.EachClient(cl =>
                {
                    lstDev.Add(cl.GetDeviceInfoEx());
                });
            }
            else
            {
                lstDev = await Program.sipServer.DB.GetDeviceInfoList(start, end);
            }
            return View(lstDev);
        }
        public async Task<IActionResult> Channels(string DeviceID)
        {
            var model = await Program.sipServer.DB.GetChannelList(DeviceID);
            return View(model);
        }

        public async Task<IActionResult> Delete(string DeviceID)
        {
            return View(await Program.sipServer.DB.GetDeviceInfo(DeviceID));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string DeviceID, IFormCollection collection)
        {
            await Program.sipServer.DB.DeleteDeviceInfo(DeviceID);
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
