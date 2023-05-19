using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IStoreServices
    {
        List<PawnStoreModels> LoadDataStore(string strKeyword, int intIsActive, int intPageSize, int intPageIndex);
        PawnStoreModels LoadDetailStore(int intStoreId);
        MessageModels AddStore(PawnStoreModels pawnStore);
        int DeleteStore(int idStore, string strUserDelete);
        PawnStoreModels GetStoreByDefault(int userId);
        IEnumerable<PawnStoreModels> GetListStoreByUser(int userId);
        List<PawnStoreModels> LoadAllStore();
    }
}
