using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    [HandleError]
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult _404(string aspxerrorpath)
        {
            if (!string.IsNullOrWhiteSpace(aspxerrorpath))
                return RedirectToAction("_404");
            return View();
        }

        // GET: Error
        public ActionResult _403(string aspxerrorpath)
        {
            if (!string.IsNullOrWhiteSpace(aspxerrorpath))
                return RedirectToAction("_403");
            return View();
        }

        public ActionResult _401()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

         protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}