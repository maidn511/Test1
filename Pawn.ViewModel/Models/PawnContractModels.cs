using Pawn.ViewModel.Interfaces;
using Pawn.Libraries;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pawn.ViewModel.Models
{
    public class PawnContractModels : BaseModels, IDocumentModals
    {

        public PawnContractModels()
        {
            ListExtentionContract = new List<ExtentionContractModels>();
            CreatedDate = DateTime.Now;
        }


        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double TotalMoney { get; set; }
        public string Collateral { get; set; } // Tài sản thế chấp
        public double InterestRate { get; set; } // Lãi suất
        public int? InterestRateType { get; set; } //Hình thức lãi suất
        public string InterestRateTypeString
        {
            get
            {
                switch (this.InterestRateType)
                {
                    case (int)MethodTypeEnum.MoneyPerDay:
                        return "Ngày";
                    case (int)MethodTypeEnum.MoneyPerWeek:
                        return "Tuần";
                    case (int)MethodTypeEnum.PercentPerMonth30:
                        return "Tháng";
                    case (int)MethodTypeEnum.PercentPerMonthDay:
                        return "Tháng";
                    case (int)MethodTypeEnum.PercentPerWeek:
                        return "Tuần";
                    default:
                        return "";
                }
            }
        }
        public double LaiDenNgay { get; set; } // Lãi đến ngày
        public double LaiDaDong { get; set; } // lãi đã đóng
        public decimal NoCu { get; set; }
        public bool IsBefore { get; set; } //Thu lãi trước
        public double TongLai { get; set; }
        public int? InterestRateOption { get; set; } //Loại lãi k/1tr || k/1 ngày

        public string InterestRateOptionString
        {
            get
            {
                switch (this.InterestRateOption)
                {
                    case (int)CapitalRateTypeEnum.KperDay:
                        return "k /ngày";
                    case (int)CapitalRateTypeEnum.KPerMillion:
                        return "k /1 triệu";
                    case (int)CapitalRateTypeEnum.KPerWeek:
                        return "k /tuần";
                    case (int)CapitalRateTypeEnum.PercentPerMonth:
                        return "% /1 tháng";
                    case (int)CapitalRateTypeEnum.PercentPerMonthPeriodic:
                        return "% /1 tháng";
                    case (int)CapitalRateTypeEnum.PercentPerWeek:
                        return "% /1 tuần";
                    default:
                        return "";
                }
            }
        }
        public int? PawnDateNumber { get; set; } //Số ngày vay

        public int? InterestRateNumber { get; set; } //Kỳ lãi
        public DateTime? NgayPhaiDongLai { get; set; } //Ngày phải đóng lãi
        public string NgayPhaiDongLaiString
        {
            get
            {
                if (this.NgayPhaiDongLai == Convert.ToDateTime("01/01/1900"))
                    return "Hoàn thành";
                else
                    return NgayPhaiDongLai?.ToString(Constants.DateFormat);
            }
        } //Ngày phải đóng lãi
        public DateTime PawnDate { get; set; } //Ngày vay
        public string PawnDateString => PawnDate.ToString("dd-MM-yyyy");
        public string PawnDatePostString { get; set; }
        public DateTime PawnDatePost => String.IsNullOrEmpty(PawnDatePostString)?DateTime.Now.Date: DateTime.ParseExact(PawnDatePostString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        public double? NgayLaiDaDong { get; set; } //Ngày lãi đã đóng
        public double? NgayLaiHomNay { get; set; } //Ngày lãi đên hôm nay

        public string Note { get; set; }

        public bool IsCloseContract { get; set; }
        public bool IsBadDebt { get; set; }

        public DateTime? ClosedDate { get; set; }

        public string ClosedUser { get; set; }

        public string Code { get; set; }

        public string DocumentName { get; set; }

        public int DocumentType { get; set; }
        public int NumOfDays { get; set; }
        public double TienLaiMotNgay { get; set; }
        public string DocumentDateString => DocumentDate.ToString("MM/dd/yyyy");
        public DateTime DocumentDate {get;set;}
        public DateTime DocumentDatePost
        {
            get
            {
                DateTime dateFormat = new DateTime();
                if (!string.IsNullOrEmpty(DocumentDateString))
                    dateFormat = DocumentDateString.ToDateTime(Constants.DateFormat);
                return dateFormat;
            }
        }
        public int? Status { get; set; }
        public string StatusString
        {
            get
            {
                switch (this.Status)
                {
                    case (int)PawnStatusEnum.DangVay:
                        return "Đang vay";
                    case (int)PawnStatusEnum.NoLai:
                        return "Nợ lãi";
                    case (int)PawnStatusEnum.QuaHan:
                        return "Quá hạn";
                    case (int)PawnStatusEnum.TraGoc:
                        return "Trả góc";
                    default:
                        return "Đang vay";
                }
            }
        }
        public double TienVayThem { get; set; }
        public bool IsSystem { get; set; }
        public string IdentityCard { get; set; }
        public string ICCreateDate { get; set; } //Ngày cấp cmnd
        public string ICCreatePlace { get; set; } //Nơi cấp
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime ToDateNotChange { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public bool IsNotification { get; set; }
        public List<ExtentionContractModels> ListExtentionContract { get; set; }
        public bool IsPaid { get; set; }
        public decimal SumTotalMoneyPaid { get; set; }
        public decimal SumTotalMoney { get; set; }
        public double TongTienLai { get; set; }

        public decimal? MoneyServices { get; set; }
        public decimal? MoneyIntroduce { get; set; }
        public int? StaffManagerId { get; set; }
    }

    public class Pay
    {
        public int SoLanTraChan { get; set; }
        public int SoLanTraLe { get; set; }
        public int SoNgayVay { get; set; }
        public DateTime NgayBatDau { get; set; }
        public double TienLai { get; set; }
        public int ValueMethod { get; set; }
    }
}
