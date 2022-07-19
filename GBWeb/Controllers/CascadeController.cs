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
    public class CascadeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public CascadeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await Program.sipServer.DB.GetSuperiorList());
        }
        private void FillClientSetting(SuperiorInfo info, IFormCollection collection)
        {
            //TODO:检查值是否正确
            info.Name = collection["Name"];
            info.ServerID = collection["ServerID"];
            info.Server = collection["Server"];
            info.ServerPort = Convert.ToInt32(collection["ServerPort"]);
            info.ClientID = collection["ClientID"];
            info.ClientName = collection["ClientName"];
            info.SIPUsername = collection["SIPUsername"];
            info.SIPPassword = collection["SIPPassword"];
            info.Expiry = Convert.ToInt32(collection["Expiry"]);
            info.RegSec = Convert.ToDouble(collection["RegSec"]);
            info.HeartSec = Convert.ToDouble(collection["HeartSec"]);
            info.HeartTimeoutTimes = Convert.ToInt32(collection["HeartTimeoutTimes"]);
            info.Enable = collection["Enable"][0] == "true";
            info.UseTcp = collection["UseTcp"][0] == "true";

        }

        public async Task<ActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            SuperiorInfo sinfo = new SuperiorInfo();
            try
            {
                FillClientSetting(sinfo, collection);
                sinfo.ID = Guid.NewGuid().ToString();
                await Program.sipServer.Cascade.Add(sinfo);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
            }
            return View(sinfo);
        }


        public async Task<ActionResult> Edit(string id)
        {
            return View(await Program.sipServer.DB.GetSuperiorInfo(id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, IFormCollection collection)
        {
            SuperiorInfo sinfo = null;
            try
            {
                sinfo = await Program.sipServer.DB.GetSuperiorInfo(id);
                FillClientSetting(sinfo, collection);
                await Program.sipServer.Cascade.Update(sinfo);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
            }
            return View(sinfo);
        }

        public async Task<ActionResult> Delete(string id)
        {
            return View(await Program.sipServer.DB.GetSuperiorInfo(id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, IFormCollection collection)
        {
            try
            {
                await Program.sipServer.Cascade.Remove(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public async Task<ActionResult> Details(string id)
        {
            return View(await Program.sipServer.DB.GetSuperiorInfo(id));
        }
    }
}
