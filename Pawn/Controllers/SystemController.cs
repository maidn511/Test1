using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.Models;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class SystemController : BaseController
    {
        // GET: System
        private IMenuServices _menuServices;
        private IRoleServices _roleServices;
        private IAccountServices _pawnAccount;

        public SystemController(IMenuServices menuServices, IRoleServices roleServices, IAccountServices pawnAccount)
        {
            _menuServices = menuServices;
            _roleServices = roleServices;
            _pawnAccount = pawnAccount;
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ListMenu()
        {
            var lstMenu = _menuServices.LoadDataMenu(-1, null, -1, -1, 99999, 1).OrderBy(x => x.ParentId).ToList();
            List<Models.MenuModels> lst = new List<Models.MenuModels>();
            for (int i = 0; i < lstMenu.Count; i++)
            {
                if (lstMenu[i].ParentId == 0)
                {
                    Models.MenuModels c = lstMenu[i].Map<Models.MenuModels>();
                    c.LstMenuChild = new List<Models.MenuModels>();
                    for (int j = i + 1; j < lstMenu.Count; j++)
                    {
                        if (lstMenu[j].ParentId == c.Id && lstMenu[j].ParentId != 0)
                        {
                            c.LstMenuChild.Add(lstMenu[j].Map<Models.MenuModels>());
                        }
                    }
                    lst.Add(c);
                }

            }

            return View(lst);
        }

        [HttpPost]
        public JsonResult GetDetailMenu(int id)
        {
            Models.MenuModels model = new Models.MenuModels();
            var lstMenu = _menuServices.LoadDataMenu(-1, null, -1, -1, 99999, 1).Where(x => x.ParentId == 0).ToList();
            if (id > 0)
            {
              
                var menu = _menuServices.GetMenuDetail(id);
                if (menu != null)
                {
                    model = menu.Map<Models.MenuModels>();
                 
                }
            }
            ViewBag.LstMenuParent = lstMenu;
            return Json(this.RenderPartialViewToString("_DetailMenuModel", model));
        }

        [HttpPost]
        public ActionResult ManageMenu(Models.MenuModels model)
        {
            CustomJsonResult result = new CustomJsonResult();
            try
            {
                if (model.Id > 0)
                {
                    model.UpdatedDate = DateTime.Now;
                    model.UpdatedUser = RDAuthorize.Username;
                    model.Url = "/" + model.Controller + "/" + model.Action;
                    _menuServices.UpdateMenu(model.Map<ViewModel.Models.MenuModels>());
                }
                else
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedUser = RDAuthorize.Username;
                    model.IsDeleted = false;
                    model.RoleId = 0;
                    model.IsCms = true;
                    _menuServices.InsertMenu(model.Map<ViewModel.Models.MenuModels>());
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }

        public ActionResult Permission()
        {
            var model = new MenuPermissionModel();
            var lstRole = _roleServices.LoadDataRole("", 1, -1, 1);
            foreach (var r in lstRole)
            {
                model.Roles.Add(new BaseItemModel { Id = r.Id, Name = r.RoleName });
            }
            var lstMenu = _menuServices.LoadDataMenu(-1, null, -1, -1, 99999, 1);
            var dicMenuPemission = _menuServices.GetLstMenuByLstRoleId_Dic(lstRole.Select(x => x.Id).ToList());
            bool allowed = false;
            foreach (var m in lstMenu)
            {
                foreach (var r in lstRole)
                {
                    allowed = false;
                    if (dicMenuPemission.ContainsKey(r.Id))
                    {
                        allowed = dicMenuPemission[r.Id].Contains(m.Id);
                    }
                    if (!model.Allowed.ContainsKey(r.Id))
                    {
                        //model.Allowed.Add(r.Id, new Dictionary<int, bool>());
                        model.Allowed[r.Id] = new Dictionary<int, bool>();
                    }
                    model.Allowed[r.Id][m.Id] = allowed;
                }
                //model.Menus.Add(new BaseItemModel { Id = m.Id, Name = $"{m.Controller} / { m.Action }", Description = m.Description });
                model.Menus.Add(new BaseItemModel { Id = m.Id, Name = $"{m.MenuName} ({m.Controller} / { m.Action })", Description = m.Description, ParentID = m.ParentId });

            }
        
            return View(model);
        }

  


        [HttpPost]
        public ActionResult Permission(FormCollection form)
        {
            var result = new CustomJsonResult();
            try
            {
                var lstRole = _roleServices.LoadDataRole("", 1, -1, 1);
                List<int> lstMenuId = new List<int>();
                foreach (var r in lstRole)
                {
                    var formKey = "allow_" + r.Id;
                    var menuPermission = !string.IsNullOrEmpty(form[formKey])
                        ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>();
                    lstMenuId = new List<int>();
                    if (menuPermission.Any())
                    {
                        foreach (var m in menuPermission)
                        {
                            lstMenuId.Add(int.Parse(m));
                        }
                    }
                    _menuServices.MapMenuToRole(r.Id, lstMenuId);

                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }

        public ActionResult AddRoleToUser()
        {
           
            int userID = RDAuthorize.UserId;
            if (RDAuthorize.IsRoot)
            {
                ViewBag.ListUser = _pawnAccount.GetAllData().Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Username
                }).ToList();
                ViewBag.ListRole = _roleServices.LoadDataRole("", 1, -1, 1).Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.RoleName
                }).ToList();
            }
            else
            {
                ViewBag.ListUser = _pawnAccount.GetListAccountByLevel(userID).Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Username
                }).ToList();
                ViewBag.ListRole = _roleServices.LoadRoleLevelByUser(userID).Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.RoleName
                }).ToList();
            }
            
            return View();
        }

        public JsonResult GetRoleLevelByUSer()
        {
            int userID = RDAuthorize.UserId;
            var result = _roleServices.LoadRoleLevelByUser(userID);
            return Json(result);
        }

        public JsonResult GetRoleByUser(int userId)
        {
            var lstRoleId = _roleServices.GetLstRoleIdByUserId(userId).FirstOrDefault();
            return Json(lstRoleId);
        }

        public JsonResult UpdateUserRole()
        {
            var userId = Request.Form["slUserId"];
            var roleId = Request.Form["slRoleId"];
            int temp = 0;
            UserRoleModels userRole = new UserRoleModels()
            {
                AccountId = int.TryParse(userId.ToString(), out temp) ? int.Parse(userId.ToString()) : -1,
                RoleId = int.TryParse(roleId.ToString(), out temp) ? int.Parse(roleId.ToString()) : -1
            };
            int result = _roleServices.AddRoleV2(userRole);
            return Json(result);
        }
    }
}