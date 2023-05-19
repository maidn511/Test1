using System;
using System.Collections.Generic;

namespace Pawn.Libraries
{
    public class Calculator
    {
        public static string ReturnFromToDate(int intMethod, DateTime dt, int intValue)
        {
            string todate = "";
            CapitalMethodEnum capitalMethod = (CapitalMethodEnum)intMethod;
            switch (capitalMethod)
            {
                case CapitalMethodEnum.LaiNgay:
                    todate = dt.AddDays(intValue - 1).ToString(Constants.DateFormat);
                    break;
                case CapitalMethodEnum.LaiThang:
                    todate = dt.AddDays(intValue * 30).AddDays(-1).ToString(Constants.DateFormat);
                    break;
                case CapitalMethodEnum.LaiThangDinhKy:
                    todate = dt.AddMonths(intValue).ToString(Constants.DateFormat);
                    break;
                case CapitalMethodEnum.LaiTuanPT:
                case CapitalMethodEnum.LaiTuanK:
                    todate = dt.AddDays(intValue * 7).AddDays(-1).ToString(Constants.DateFormat);
                    break;
                default:
                    break;
            }
            return todate;
        }


        public static string GetNameOfRateType(int intRateType)
        {
            CapitalRateTypeEnum capitalRateType = (CapitalRateTypeEnum)intRateType;
            string ratetype = "";
            switch (capitalRateType)
            {
                case CapitalRateTypeEnum.KPerMillion:
                    ratetype = "k/1 triệu";
                    break;
                case CapitalRateTypeEnum.KperDay:
                    ratetype = "k/1 ngày";
                    break;
                case CapitalRateTypeEnum.PercentPerMonth:
                case CapitalRateTypeEnum.PercentPerMonthPeriodic:
                    ratetype = "%/1 tháng";
                    break;
                case CapitalRateTypeEnum.PercentPerWeek:
                    ratetype = "% /1 tuần (VD : 2% / 1 tuần)";
                    break;
                case CapitalRateTypeEnum.KPerWeek:
                    ratetype = "k/1 tuần (VD : 100k / 1 tuần)";
                    break;
                default:
                    break;
            }
            return ratetype;
        }

        public static int GetDayToAdd(int borrowNumber, int intMethod, DateTime? dt = null)
        {
            var day = 0;
            CapitalMethodEnum capitalMethod = (CapitalMethodEnum)intMethod;
            switch (capitalMethod)
            {
                case CapitalMethodEnum.LaiNgay:
                    day = borrowNumber;
                    break;
                case CapitalMethodEnum.LaiThang:
                    day = borrowNumber * 30;
                    break;
                case CapitalMethodEnum.LaiThangDinhKy:
                    var dt1 = dt.Value;
                    var dt2 = dt1.AddMonths(1);
                    day = (int)(dt2 - dt1).TotalDays + 1;
                    break;
                case CapitalMethodEnum.LaiTuanPT:
                case CapitalMethodEnum.LaiTuanK:
                    day = borrowNumber * 7;
                    break;
                default:
                    break;
            }
            return day;
        }
    }
}
