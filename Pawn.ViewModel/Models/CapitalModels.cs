using Pawn.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class CapitalModels
    {
        public CapitalModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public int StoreId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DocumentName { get; set; }
        public int? DocumentType { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Phone { get; set; }
        public decimal MoneyNumber { get; set; }
        public int Method { get; set; }
        public bool IsBeforeReceipt { get; set; }
        public decimal? InterestRate { get; set; }
        public int? RateType { get; set; }
        public int? BorrowNumber { get; set; }
        public int? BorrowPeriod { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedUser { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int? StatusContractId { get; set; }
        public int? Status { get; set; }
        public bool IsSystem { get; set; }
        public bool IsClose { get; set; }
        public DateTime? CloseDate { get; set; }
        public string CloseUser { get; set; }
        public int TotalRows { get; set; }
        public decimal MoneyPaid { get; set; }
        public string ActionModal { get; set; }
        public string StatusName { get; set; }
        public string DocumentDateString { get; set; }
        public DateTime? DocumentDatePost
        {
            get
            {
                DateTime? dateFormat = null;
                if (!string.IsNullOrEmpty(DocumentDateString))
                    dateFormat = DocumentDateString.ToDateTimeNull(Constants.DateFormat);
                return dateFormat;
            }
        }
        public string RateTypeName { get; set; }
        public DateTime LoanDate { get; set; }
        public bool IsPaid { get; set; }
        public int StatusType { get; set; }
    }

    public class CapitalDetailModels
    {
        public CapitalDetailModels()
        {
            ListCapitalPayDay = new List<CapitalPayDayModels>();
            ListCapitalLoan = new List<CapitalLoanModels>();
            ListExtentionContract = new List<ExtentionContractModels>();
        }
        public decimal TotalMoney { get; set; }
        public decimal TotalMoneySys { get; set; }
        public decimal TotalInterest { get; set; }
        public string FromDate { get; set; }
        public string CustomerName { get; set; }
        public string ToDate { get; set; }
        public decimal Paid { get; set; }
        public decimal OldDebts { get; set; }
        public string Status { get; set; }
        public decimal? InterestRate { get; set; } //phần trăm tiền lãi or số tiền lãi
        public DateTime DocumentDate { get; set; }
        public int Id { get; set; }
        public int Method { get; set; }
        public int? BorrowNumber { get; set; } //số ngày, tuần, tháng vay
        public int? RateType { get; set; } //Loại lãi suất
        public string MethodName { get; set; }
        public int? BorrowPeriod { get; set; } //kỳ lãi
        public decimal MoneyPerDay { get; set; } //
        public decimal TotalMoneyOrther { get; set; }
        public string CapitalPayDayDate { get; set; }
        public decimal CapitalLoanWithdrawn { get; set; } // So tien lai cua so tien da rut von
        public decimal MoneyPaid { get; set; }
        public bool IsBeforeReceipt { get; set; }
        public int StatusType { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime LastLoanDate { get; set; }
        public List<CapitalPayDayModels> ListCapitalPayDay { get; set; }
        public List<CapitalLoanModels> ListCapitalLoan { get; set; }
        public List<ExtentionContractModels> ListExtentionContract { get; set; }
    }
}
