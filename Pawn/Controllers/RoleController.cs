using Pawn.Authorize;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class RoleController : BaseController
    {

        private IRoleServices _role;
        public RoleController(IRoleServices role)
        {
            _role = role;
        }
        // GET: Role
        public ActionResult Index()
        {
            if (!RDAuthorize.IsPermissionConfig("RoleView")) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        public ActionResult AddRole(int? id)
        {
            if (id.HasValue) if (!RDAuthorize.IsPermissionConfig("RoleEdit")) return RedirectToAction("Index");
            if (!id.HasValue) if (!RDAuthorize.IsPermissionConfig("RoleAdd")) return RedirectToAction("Index");
            if (!id.HasValue) return View(new RoleModels());
            var objRole = _role.LoadDetailRole(id.Value);
            return View(objRole);
        }

        [HttpPost]
        public ActionResult AddRole(RoleModels objRole)
        {
            if (objRole.Id > 0) if (!RDAuthorize.IsPermissionConfig("Roledit")) return RedirectToAction("Index");
            if (objRole.Id <= 0) if (!RDAuthorize.IsPermissionConfig("RoleAdd")) return RedirectToAction("Index");
            objRole.CreatedDate = DateTime.Now;
            objRole.CreatedUser = RDAuthorize.Username;
            var isError = _role.AddRole(objRole);
            return Json(isError);
        }

        public ActionResult _PartialRole(string strKeyword = "", int intIsActive = -1, int intPageIndex = 1)
        {
            var data = _role.LoadDataRole(strKeyword, intIsActive, pageSize, intPageIndex);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = pageSize * (intPageIndex - 1) + 1;
            return PartialView(data);
        }

        public ActionResult _PartialRoleModal()
        {
            var data = _role.LoadDataRole("", 1, -1, 1);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = 1;
            return PartialView(data);
        }

        public ActionResult DeleteRole(int intRoleId)
        {
            if (!RDAuthorize.IsPermissionConfig("RoleDelete")) return Json(true);
            var data = _role.DeleteRole(intRoleId, RDAuthorize.Username);
            return Json(data);
        }
    }
}