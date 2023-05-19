using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Mapper;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class LoginController : Controller
    {
        private IAccountServices _pawnAccount;
        private IRoleServices _roleService;
        private IStoreServices _store;
        public LoginController(IAccountServices pawnAccount, IRoleServices roleService, IStoreServices store)
        {
            _pawnAccount = pawnAccount;
            _roleService = roleService;
            _store = store;
        }
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string returnUrl)
        {
            
            if (!string.IsNullOrEmpty(RDAuthorize.Username))
            {
                if (Request.Url != null)
                    return Redirect("/");
            }
            Session[Constants.returnUrl] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountModels model)
        {
            ViewBag.Username = model.Username;
            ViewBag.Error = "";
            if (ModelState.IsValid)
            {
                var urlContinue = Session[Constants.returnUrl] + "";
                model.Password = Encryption.Md5Encryption(model.Password);
                var account = _pawnAccount.Login(model);
                if (account != null)
                {
                    var lstRoleId = _roleService.GetLstRoleIdByUserId(account.Id);
                    account.ListRole = lstRoleId;
                    
                    if (lstRoleId.Contains(RoleEnum.Root))
                    {
                        account.ListStores = _store.LoadAllStore();
                        account.Store = account.ListStores.OrderBy(s => s.Id).FirstOrDefault();
                        Session[Constants.isRoot] = true;
                    }
                    RDAuthorize.Set(account);
                    if (!string.IsNullOrEmpty(urlContinue))
                    {
                        Session.Remove(Constants.returnUrl);
                        return Redirect(urlContinue);
                    }
                    return Redirect("/");
                }
                else
                {
                    ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                }
            }
            else
                ViewBag.Error = "Có lỗi xảy ra!";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login");
        }

    }
}