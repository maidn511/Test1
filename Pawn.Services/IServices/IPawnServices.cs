using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pawn.Libraries;
using Pawn.ViewModel.Models;

namespace Pawn.Services
{
    public interface IPawnServices
    {
        #region Code A Tuấn Em
        MessageModels AddPawnContract(PawnContractModels vlModel);
        PawnContractModels LoadDetailPawnContract(int idContract);
        MessageModels DeletePawn(int idPawn);
        List<PawnContractModels> LoadDataCustomerPaidTomorrow(InterestPaid addDay);
        MessageModels UpdateBadDebt(int idVayLai, bool isBadDebt);
        #endregion

        #region Code A Nam
        IEnumerable<PawnContractModels> GetAllData(int storeId, DateTime? fromDate, DateTime? toDate,
         int currentPage, int pageSize, int? customerId, string docName, int? status, string sortcolumn, string sorttype,int staffId);

        PawnContractModels GetDetailById(int storeId, int contractId);

        IEnumerable<PawnPayModels> GetPawnPaysById(int id);

        MessageModels UpdateHopDong(PawnContractModels header);

        MessageModels DongLai(PawnContractModels header, PawnPayModels line);
        MessageModels DongLaiTuyBien(PawnContractModels header, PawnPayModels line);
        MessageModels TraBotGoc(PawnContractModels header);
        MessageModels DongHopDong(int contractId, double totalMoney);
        MessageModels GiaHanThem(PawnContractModels header, int day);
        string GetMaxVLContract();
        MemoryStream ExportExcel();
        MessageModels ConvertStore(int vlId, int storeId);
        MessageModels UpdateMoneySI(int contractId, decimal? moneyIntroduce, decimal? moneyService);
        #endregion

    }
}
