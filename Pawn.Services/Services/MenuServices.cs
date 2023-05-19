using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Logger;
using Pawn.ViewModel.Models;

namespace Pawn.Services
{
    public class MenuServices : IMenuServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public MenuServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public MenuModels GetMenuDetail(int menuID)
        {
            try
            {
                var menu = _unitOfWork.MenuRepository.Find(s => s.Id == menuID);
                var menuModel = _mapper.Map<Tb_Menu, MenuModels>(menu);
                return menuModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> PawnMenuModels ", ex);
                return new MenuModels();
            }
        }

        public int InsertMenu(MenuModels menu)
        {
            int result = 0;
            try
            {
                var menuModel = _mapper.Map<MenuModels, Tb_Menu>(menu);
                _unitOfWork.MenuRepository.Insert(menuModel);
                result = _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> LoadDataMenu ", ex);
            }

            return result;
        }

        public List<MenuModels> LoadDataMenu(int intParentId, string strKeyword, int intIsActive, int intIsCms, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { intParentId, strKeyword, intIsActive, intIsCms, intPageSize, intPageIndex - 1 };
                var lstData = _unitOfWork.ExecStoreProdure<MenuModels>("SP_MENU_SRH {0}, {1}, {2}, {3}, {4}, {5}", _params).ToList();
                return lstData;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> LoadDataMenu ", ex);
                return new List<MenuModels>();
            }
        }


        public int UpdateMenu(MenuModels menu)
        {

            int result = 0;
            try
            {
                var detailMenu = _unitOfWork.MenuRepository.FindById(menu.Id);
                detailMenu.MenuName = menu.MenuName;
                detailMenu.Description = menu.Description;
                detailMenu.Controller = menu.Controller;
                detailMenu.Action = menu.Action;
                detailMenu.ParentId = menu.ParentId;
                detailMenu.OrderIndex = menu.OrderIndex;
                detailMenu.IsActive = menu.IsActive;
                detailMenu.IsShow = menu.IsShow;
                detailMenu.Url = menu.Url;
                result = _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> LoadDataMenu ", ex);
            }

            return result;
        }

        public Dictionary<int, List<int>> GetLstMenuByLstRoleId_Dic(List<int> lstRoleId)
        {
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            try
            {
                result = _unitOfWork.MenuPermissionRepository
                    .Filter(x => lstRoleId.Contains(x.RoleId))
                    .GroupBy(x => x.RoleId)
                    .ToDictionary(x => x.Key, (x => x.Select(m => m.MenuId).ToList()));

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> GetLstMenuByLstRoleId_Dic ", ex);
                return null;
            }
            return result;
        }

        public int MapMenuToRole(int roleId, List<int> lstMenuId)
        {
            int result = 0;
            try
            {
                if (lstMenuId == null || !lstMenuId.Any())
                {
                    var lstRemove = _unitOfWork.MenuPermissionRepository.Filter(x => x.RoleId == roleId);
                    if (lstRemove != null && lstRemove.Any())
                    {
                        _unitOfWork.MenuPermissionRepository.Delete(x => x.RoleId == roleId);
                        _unitOfWork.SaveChanges();
                    }
                }
                else
                {
                    var lstOldMenu = _unitOfWork.MenuPermissionRepository.Filter(x => x.RoleId == roleId).Select(x => x.MenuId).ToList();
                    // thêm role
                    if (lstOldMenu == null || !lstOldMenu.Any())
                    {
                        foreach (var r in lstMenuId)
                        {
                            _unitOfWork.MenuPermissionRepository.Insert(new Tb_MenuPermission { MenuId = r, RoleId = roleId });
                        }
                        _unitOfWork.SaveChanges();
                    }
                    // cập nhật, xóa role
                    else
                    {
                        bool bChange = false;
                        foreach (var o in lstOldMenu)
                        {
                            if (!lstMenuId.Contains(o))
                            {
                                var r = _unitOfWork.MenuPermissionRepository.Find(x => x.MenuId == o && x.RoleId == roleId);
                                if (r != null)
                                {
                                    bChange = true;
                                    _unitOfWork.MenuPermissionRepository.Delete(r);
                                }
                            }
                        }
                        foreach (var o in lstMenuId)
                        {
                            if (!lstOldMenu.Contains(o))
                            {
                                bChange = true;
                                _unitOfWork.MenuPermissionRepository.Insert(new Tb_MenuPermission { MenuId = o, RoleId = roleId });

                            }
                        }
                        if (bChange)
                        {
                            _unitOfWork.SaveChanges();
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> MapMenuToRole ", ex);
                return -1;
            }

            return result;

        }

        public List<MenuModels> GetLstMenuByLstRoleId(List<int> lstRoleId)
        {
            List<MenuModels> result = new List<MenuModels>();
            try
            {
                var lstmenuId = _unitOfWork.MenuPermissionRepository.Filter(x => lstRoleId.Contains(x.RoleId)).Select(x => x.MenuId).Distinct().ToList();
                if (lstmenuId != null && lstmenuId.Any())
                {
                    result = _unitOfWork.MenuRepository.Filter(x => lstmenuId.Contains(x.Id) && x.IsActive == true && !x.IsDeleted)
                        .Select(x => new MenuModels()
                        {
                            Action = x.Action,
                            Controller = x.Controller,
                            IsActive = x.IsActive

                        }).ToList();
                }
                return result;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> GetLstMenuByLstRoleId_Dic ", ex);
                return new List<MenuModels>();
            }
        }

        public List<MenuModels> LoadDataMenuByUser(long UserID)
        {
            try
            {
                var _params = new object[] { UserID };
                var lstData = _unitOfWork.ExecStoreProdure<MenuModels>("SP_MENU_SRH_BY_ROLE {0}", _params).ToList();
                return lstData;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnMenuServices --> LoadDataMenuByUser ", ex);
                return new List<MenuModels>();
            }
        }
    }
}
