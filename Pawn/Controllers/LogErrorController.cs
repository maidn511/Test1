using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class LogErrorController : Controller
    {
        int pageSize = 10;
        // GET: LogError
        public ActionResult Index()
        {
            var strDerectory = ConfigurationManager.AppSettings["ConfigLogPath"];
            var listFile = Directory.GetFiles(strDerectory);
            return View();
        }

        public ActionResult _PartialData(int page = 1)
        {
            var strDerectory = ConfigurationManager.AppSettings["ConfigLogPath"];
            var listFile = Directory.GetFiles(strDerectory).Select(c => new FileInfo(c)).ToList().Where(s => System.IO.File.ReadLines(s.FullName).Count() > 0);
            ViewBag.TotalRows = listFile.ToList().Count;
            var lstFileInfo = listFile.OrderByDescending(c => c.CreationTime).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Index = (pageSize * (page - 1)) + 1;
            return PartialView(lstFileInfo);
        }

        public string GetError(string path)
        {
            var text = System.IO.File.ReadLines(path);
            return text != null && text.Count() > 0 ? text.First() : "";
        }

        public string GetFullError(string path)
        {
            return System.IO.File.ReadAllText(path).Replace("\n", "<br />").Replace("   ", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
        }
    }
}