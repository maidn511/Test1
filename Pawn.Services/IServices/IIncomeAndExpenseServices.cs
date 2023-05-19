using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IIncomeAndExpenseServices
    {
        IEnumerable<IncomeAndExpenseModels> GetAllData(int storeId,int voucherType, int? method, DateTime? fromDate, DateTime? toDate, int currentPage, int pageSize);
        List<IncomeAndExpenseModels> LoadDataIncomeAndExpense(string strKeyword, int intIsActive, int intPageSize, int intPageIndex);
        MessageModels AddIncomeAndExpense(IncomeAndExpenseModels pawnIncomeAndExpense, int type);
        MessageModels DeleteIncomeAndExpense(int idIncomeAndExpense, string strUserDelete);
    }
}
