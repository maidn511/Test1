using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Pawn.Fillters
{
    public class AuthAttribute : AuthorizeAttribute
    {
        public IAccountServices _accountService { get; set; }
        public IMenuServices _menuService { get; set; }
        public IRoleServices _roleService { get; set; }


        //Action and controller non check perrmission
        private List<string> _lstActionNoneCheck = new List<string> { "login", "logout", "register", "changepassword", "index" };
        private List<string> _lstControllerNoneCheck = new List<string> { "common", };
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.IsChildAction)
            {
                string controller = filterContext.RouteData.Values["controller"].ToString().ToLower();
                string action = filterContext.RouteData.Values["action"].ToString().ToLower();
                string returnUrl = filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri;


                if (!((controller.Equals("home") && _lstActionNoneCheck.Contains(action)) || _lstControllerNoneCheck.Contains(controller)))
                {
                    var username = HttpContext.Current.Session[Constants.username];
                    if (username == null)
                    {
                        // Redirect to Login Page
                        FormsAuthentication.SignOut();

                        HttpContext.Current.Session[Constants.returnUrl] = filterContext.HttpContext.Request.Url;

                        filterContext.Result = new RedirectResult("~/Login");

                    }
                    else //nếu đang còn session
                    {
                        var user = _accountService.GetAccountByUserName(username.ToString());
                        if (user != null && user.IsActive)
                        {

                            var isAllowAccess = false; // accept action
                            bool IsRoot = false; 
                            bool IsAdmin = false;
                            var lstRoleId = _roleService.GetLstRoleIdByUserId(user.Id);
                            if (lstRoleId != null && lstRoleId.Any())
                            {
                                if (lstRoleId.Contains(RoleEnum.Admin))
                                {
                                    IsAdmin = true;

                                }
                               
                                if (lstRoleId.Contains(RoleEnum.Root))
                                {
                                    isAllowAccess = true;
                                    IsAdmin = true;
                                    IsRoot = true;
                                }
                                else
                                {
                                    var lstMenu = _menuService.GetLstMenuByLstRoleId(lstRoleId);
                                    if (lstMenu != null && lstMenu.Any())
                                    {
                                        MenuModels menu = null;
                                        if (action == "index")
                                        {
                                            menu = lstMenu.FirstOrDefault(x => (!string.IsNullOrEmpty(x.Controller)
                                                                                      && x.Controller.ToLower().Equals(controller)));
                                        }
                                        else
                                        {
                                            isAllowAccess = true;
                                            menu = lstMenu.FirstOrDefault(x => (!string.IsNullOrEmpty(x.Controller) && !string.IsNullOrEmpty(x.Action)
                                                                                      && x.Controller.ToLower().Equals(controller)
                                                                                      && x.Action.ToLower().Equals(action)));
                                        }
                                        if (menu != null && menu.IsActive)
                                        {
                                            isAllowAccess = true;
                                        }

                                    }
                                }

                            }
                            if (isAllowAccess)
                            {
                                HttpContext.Current.Session[Constants.username] = username;
                                HttpContext.Current.Session[Constants.userid] = user.Id;
                                HttpContext.Current.Session[Constants.isRoot] = IsRoot;
                                HttpContext.Current.Session[Constants.isAdmin] = IsAdmin;
                            }
                            else
                            {
                                filterContext.Result = new HttpStatusCodeResult(403);
                                throw new HttpException(403, "Access Denied");
                                //filterContext.Result = new ViewResult
                                //{
                                //    ViewName = "~/Views/Shared/_401.cshtml"
                                //};
                            }
                        }
                        else
                        {
                            filterContext.Result = new RedirectResult("~/Login");
                        }
                    }
                }
                else
                {
                    var username = HttpContext.Current.Session[Constants.username];
                    if (username == null)
                    {
                        // Redirect to Login Page
                        FormsAuthentication.SignOut();

                        HttpContext.Current.Session[Constants.returnUrl] = filterContext.HttpContext.Request.Url;

                        filterContext.Result = new RedirectResult("~/Login");

                    }
                }
            }
        }
    }
}