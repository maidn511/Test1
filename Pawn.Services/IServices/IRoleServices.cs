using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IRoleServices
    {
        List<RoleModels> LoadDataRole(string strKeyword, int intIsActive, int intPageSize, int intPageIndex);
        RoleModels LoadDetailRole(int intRoleId);
        int AddRole(RoleModels pawnRole);
        int DeleteRole(int idRole, string strUserDelete);

        List<int> GetLstRoleIdByUserId(long userId);

        List<RoleModels> LoadRoleLevelByUser(int userID);
        int AddRoleV2(UserRoleModels userRole);
    }
}
