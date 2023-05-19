using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class AccountController : BaseController
    {
        private IAccountServices _pawnAccount;
        public AccountController(IAccountServices pawnAccount)
        {
            _pawnAccount = pawnAccount;
        }
        // GET: Account
        public ActionResult Index()
        {
            if (!RDAuthorize.IsPermissionConfig("AccountView")) return RedirectToAction("Index", "Home");
            return View();
        }

        public ActionResult _PartialAccount(int intAccountTypeId = -1, string strKeyword = "", int intIsActive = -1, int intPageIndex = 1)
        {
            var data = _pawnAccount.GetListAccount(strKeyword, intIsActive, pageSize, intPageIndex);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = pageSize * (intPageIndex - 1) + 1;
            return PartialView(data);
        }

        [HttpGet]
        public ActionResult AddAccount(string username)
        {
            var account = new AccountModels();
            if (!string.IsNullOrEmpty(username))
                account = _pawnAccount.LoadDetailAccount(username);
            return View(account);
        }

        [HttpPost]
        public ActionResult AddAccount(AccountModels objAccount)
        {
            if (objAccount.Id <= 0) if (!RDAuthorize.IsPermissionConfig("AccountAdd")) return RedirectToAction("Index");
            if (objAccount.Id > 0) if (!RDAuthorize.IsPermissionConfig("AccountEdit")) return RedirectToAction("Index");
            objAccount.CreatedUser = RDAuthorize.Username;
            objAccount.IsCms = true;
            objAccount.Password = Encryption.Md5Encryption(objAccount.Password);
            var rs = _pawnAccount.AddAccount(objAccount);
            return Json(rs);
        }

        public ActionResult ChangePass(string oldPass, string newPass) => Json(_pawnAccount.ChangePass(oldPass, newPass));

        public ActionResult DeleteAccount(int idUser)
        {
            var data = _pawnAccount.DeleteAccount(idUser, RDAuthorize.Username);
            return Json(data);
        }
    }
}