using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IAccountTypeServices
    {
        List<RoleModels> LoadDataAccountType(string strKeyword, int intIsActive, int intPageSize, int intPageIndex);
    }
}
