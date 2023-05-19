using Pawn.Core.DataModel;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IMenuServices
    {
        List<MenuModels> LoadDataMenu(int intParentId, string strKeyword, int intIsActive, int intIsCms, int intPageSize, int intPageIndex);
        MenuModels GetMenuDetail(int menuID);

        int InsertMenu(MenuModels menu);
        int UpdateMenu(MenuModels menu);
        Dictionary<int, List<int>> GetLstMenuByLstRoleId_Dic(List<int> lstRoleId);
        int MapMenuToRole(int roleId, List<int> lstMenuId);

        List<MenuModels> GetLstMenuByLstRoleId(List<int> lstRoleId);

        List<MenuModels> LoadDataMenuByUser(long UserID);
    }
}
