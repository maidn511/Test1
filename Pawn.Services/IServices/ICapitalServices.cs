using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface ICapitalServices
    {
        List<CapitalModels> LoadDataCapital(string strKeyword, DateTime? dtToDate, DateTime? dtFromDate, int intStatusContract, int intPageSize, int intPageIndex);
        MessageModels AddCapital(CapitalModels objCapital);
        CapitalModels LoadDetailCapital(int intCapitalId);
        CapitalDetailModels LoadCapitalDetail(int intCapitalId);
        MessageModels AddCapitalPayDay(CapitalPayDayModels objCapitalPayDays);
        MessageModels DeleteCapitalPayDay(CapitalPayDayModels objCapitalPayDays);
        decimal GetMoneyPerDayPayDay(CapitalDetailModels capitalDetail, List<CapitalLoanModels> lstCapitalLoans, DateTime dtFromDate, DateTime dtToDate);
        MessageModels CloseContract(int id, decimal moneyNumber);
    }
}
