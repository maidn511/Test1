using Pawn.ViewModel.Models;
using System.Collections.Generic;

namespace Pawn.Services
{
    public interface IAccountServices
    {
        List<AccountModels> GetListAccount(string strKeyword, int intIsActive, int intPageSize, int intPageIndex);
        AccountModels Login(AccountModels pawnAccountModels);
        AccountModels LoadDetailAccount(string strUsername);
        int AddAccount(AccountModels pawnAccount);
        IEnumerable<AccountModels> GetAllData();

        List<AccountModels> GetListAccountByStoreId();
        AccountModels GetAccountByUserName(string UserName);
        //List<RoleModels> GetLstRoleIdByUserId();
        List<AccountModels> GetListAccountByLevel(int userID);
        MessageModels ChangePass(string oldPass, string newPass);

        int DeleteAccount(int idUser, string strUserDelete);
    }
}
