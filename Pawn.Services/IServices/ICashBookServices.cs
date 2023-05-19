using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface ICashBookServices
    {
        IEnumerable<CashBookSummaryModels> GetSummaryData(int storeId, DateTime? fromDate, DateTime? toDate, string userName = "");
        IEnumerable<CashBookModals> GetAllData(int storeId, DateTime? fromDate, DateTime? toDate,
            int currentPage, int pageSize, string userName, int? docType, List<int> notes);
        int AddCashBook(CashBookModals model);
        int UpdateCashBook(CashBookModals model);
        int DeleteCashBook(string documentName, int documentType, string userName);
        MemoryStream ExportExcel(int storeId, DateTime? fromDate, DateTime? toDate,
           string userName, int? docType, List<int> notes);
    }
}
