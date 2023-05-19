using Pawn.Authorize;
using Pawn.Libraries;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pawn.Controllers
{
    [Pawn.Fillters.Auth]
    public class BaseController : Controller
    {
        public int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    if (!RDAuthorize.IsLogin)
        //    {
        //        var returnUrl = this.Request.RawUrl ?? "";
        //        //if (returnUrl.Length > 5)
        //        //{
        //        //    Regex rgx = new Regex(@"/[a-z]{2}-[a-z]{2}");
        //        //    if (rgx.IsMatch(returnUrl.Substring(0, 6).ToLower()))
        //        //    {
        //        //        returnUrl = returnUrl.Substring(6);
        //        //    }
        //        //}
        //        var url = Url.Action("Login", "Login", new { returnUrl = returnUrl });
        //        filterContext.Result = new RedirectResult(HttpUtility.UrlDecode(url));
        //        return;
        //    }
        //    base.OnActionExecuting(filterContext);
        //}


        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <returns>Result</returns>
        protected string RenderPartialViewToString()
        {
            return RenderPartialViewToString(null, null);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <returns>Result</returns>
        protected string RenderPartialViewToString(string viewName)
        {
            return RenderPartialViewToString(viewName, null);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        protected string RenderPartialViewToString(object model)
        {
            return RenderPartialViewToString(null, model);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        protected string RenderPartialViewToString(string viewName, object model)
        {
            //Original source code: http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/

            // tham khảo cách viêt: http://blog.janjonas.net/2011-06-18/aspnet-mvc3-controller-extension-method-render-partial-view-string
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }


    }
}