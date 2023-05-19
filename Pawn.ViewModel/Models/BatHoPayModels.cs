using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class BatHoPayModels
    {
        public BatHoPayModels()
        {
            CreatedDate = DateTime.Now;
            IsShowDate = false;
            IsCurrent = false;
            IsShowCurrency = false;
        }

        public int Id { get; set; }
        public int BathoId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? LoanDate { get; set; }
        public decimal? MoneyOfCustomer { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public bool IsPaid { get; set; }
        public decimal? PaymentNeedMoney { get; set; }
        public int TotalRows { get; set; }

        public double NumberOfDays { get; set; }
        public bool IsShowDate { get; set; }
        public bool IsShowCurrency { get; set; }
        public bool IsCurrent { get; set; }
    }
}
