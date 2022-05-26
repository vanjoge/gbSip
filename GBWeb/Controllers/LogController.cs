using GBWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBWeb.Controllers
{
    public class LogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Sip()
        {
            return View();
        }
        public Pager<string> GetFileLogs(int line)
        {
            List<string> data = SQ.Base.Logs.FileLogReader.GetFileLogs("LogFile/log-file.txt", line);

            Pager<string> pager = new Pager<string>();
            pager.Paging(1, data.Count, data.Count, data);
            return pager;
        }
        public string GetSipFileLogs(int line)
        {
            List<string> data = SQ.Base.Logs.FileLogReader.GetFileLogs("LogFile/siplog.txt", line);
            return string.Join("\r\n", data);
        }
    }
}
