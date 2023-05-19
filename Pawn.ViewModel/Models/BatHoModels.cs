using Pawn.Libraries;
using Pawn.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class BatHoModels : BaseModels, IDocumentModals
    {
        public BatHoModels()
        {
            CreatedDate = DateTime.Now;
            ListBatHoPay = new List<BatHoPayModels>();
        }

        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public double TotalMoney { get; set; }
        public double MoneyForGues { get; set; }
        public int LoanTime { get; set; }
        public int Frequency { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public string ToDatePaid { get; set; }
        public string PayDateString { get; set; }
        public DateTime FromDateDetail { get; set; }
        public DateTime FromDate
        {
            get
            {
                DateTime? dateFormat = null;
                if (!string.IsNullOrEmpty(FromDateString))
                    dateFormat = FromDateString.ToDateTimeNull(Constants.DateFormat);
                return dateFormat ?? DateTime.Now;
            }
            set
            {

            }
        }

        public string Note { get; set; }
        public bool IsCloseContract { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedUser { get; set; }
        public int? StaffManagerId { get; set; }
        public decimal? MoneyPerOnce { get; set; }
        public string DocumentName { get; set; }
        public int DocumentType { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Status { get; set; }
        public DateTime? PayDate { get; set; }
        public StatusContractEnum StatusContract { get; set; }
        public string StatusContractString
        {
            get
            {
                string status = "";
                switch (StatusContract)
                {
                    case StatusContractEnum.TatCaHDDangvay: status = "Đang vay"; break;
                    case StatusContractEnum.QuaHan: status = "Quá hạn"; break;
                    case StatusContractEnum.ChamHo: status = "Nợ họ"; break;
                    case StatusContractEnum.NgayMaiDongHo: status = "Ngày cuối họ"; break;
                    case StatusContractEnum.NoXau: status = "Nợ xấu"; break;
                    case StatusContractEnum.DaXoa: status = "Đã xóa"; break;
                    case StatusContractEnum.KetThuc: status = "Đóng"; break;
                    default:
                        status = "";
                        break;
                }
                return status;
            }
        }

        public string ColorContractString
        {
            get
            {
                string status = "";
                switch (StatusContract)
                {
                    case StatusContractEnum.QuaHan:
                    case StatusContractEnum.NoXau:
                        status = "background-color: #c7313e;color:#FFF"; break;
                    case StatusContractEnum.ChamHo:
                        status = " background-color: #e4a146;color:#FFF"; break;
                    default:
                        status = "";
                        break;
                }
                return status;
            }
        }
        public List<BatHoPayModels> ListBatHoPay { get; set; }
        public string CustomerName { get; set; }
        public string Code { get; set; } // Mã hợp đồng
        public string IdentityCard { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool IsSystem { get; set; }
        public bool IsPaid { get; set; }
        public decimal MoneyOrther { get; set; }
        public decimal Rate { get; set; }

        public bool IsBefore { get; set; }
        public bool IsBadDebt { get; set; }
        public decimal TotalHaving { get; set; } // Tổng số tiền thừa
        public string ICCreateDate { get; set; } //Ngày cấp cmnd
        public string ICCreatePlace { get; set; } //Nơi cấp
        public decimal? TotalPay { get; set; } // Tổng tiền đã đóng
        public decimal TotalMoneyMustPaid { get; set; }//Tổng tiền phải đóng
        public decimal DebtMoney { get; set; } // Số tiền nợ (nếu âm là tiền thừa)
        public int? PeriodHadPay { get; set; } //số kỳ đã đong
        public int? PeriodNotPay { get; set; } //số kỳ chưa đóng

        public int? NumberDate { get; set; }// Số ngày phải trả

        public decimal SumTotalMoney { get; set; }
        public decimal SumMoneyForGuest { get; set; }
        public decimal SumPaid { get; set; }
        public decimal SumMoneyMustPaid { get; set; }
        public decimal SumMoneyPerDay { get; set; }
        public decimal SumMoneyDebt { get; set; }
        public double MoneyPerDay { get; set; }
        public decimal? MoneyServices { get; set; }
        public decimal? MoneyIntroduce { get; set; }
    }
}
