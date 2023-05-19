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
    public interface IBathoServices
    {
        #region Code A T.A
        MessageModels AddBHContract(BatHoModels bhModel);
        MessageModels UpdateBatHo(BatHoModels bhModel);
        string GetMaxBHContract();
        List<BatHoModels> LoadDataBatHo(string strContractId, string strCustomerName,  DateTime? dtToDate, DateTime? dtFromDate, int? intTimePay, 
            int? intStatusID, int intPageSize, int intPageIndex, string sortcolumn, string sorttype, int staffId);
        #endregion

        #region Code A Nam
        int GetPayMaxBH(int contractId);
        MessageModels UpdateBatHo(int id);
        BatHoModels getBHById(int id);
        MessageModels CloseContract(int intBhId, decimal moneyNumberd);
        MessageModels CloseContract(int originalId, double moneyOther = 0);
        #endregion

        #region Code Tuan
        BatHoModels LoadDetailBatHoModel(int id);
        BatHoModels LoadDetailBatHo(int id);
        List<BatHoPayModels> LoadHistoryBHPay(int id);
        MessageModels BatHoPay(BatHoPayModels item, bool isClose = false, decimal moneyOrther = 0);
        int AddBatHoPay(BatHoModels item, bool isUpdate = false);
        MessageModels UpdateBatHoPay(decimal money, int idBatho);
        List<BatHoModels> LoadDataCustomerPaidTomorrow(InterestPaid addDay);
        MessageModels UpdateBadDebt(int idBatHo, bool isBadDebt);
        MessageModels DeleteBatHo(int idBatHo);
        MemoryStream ExportExcel();
        MessageModels ConvertStore(int bathoId, int storeId);
        MessageModels UpdateMoneySI(int contractId, decimal? moneyIntroduce, decimal? moneyService);
        #endregion
    }
}
