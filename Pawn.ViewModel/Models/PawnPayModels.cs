using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class PawnPayModels
    {
        public PawnPayModels()
        {
            CreatedDate = DateTime.Now;
            IsShowDate = false;
            IsCurrent = false;
            IsShowCurrency = false;
        }

        public int Id { get; set; }
        public int ContractId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? LoanDate { get; set; }
        public decimal? InterestMoney { get; set; }
        public decimal? OtherMoney { get; set; }
        public decimal? CustomerMoney { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public bool IsPaid { get; set; }
        public int TotalDay { get; set; }


        public int TotalRows { get; set; }

        public bool IsShowDate { get; set; }
        public bool IsShowCurrency { get; set; }
        public bool IsCurrent { get; set; }
    }
}
