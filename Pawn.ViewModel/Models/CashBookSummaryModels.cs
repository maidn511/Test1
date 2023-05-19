using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class CashBookSummaryModels
    {
            public string Description { get; set; }
            public decimal CreditAccount { get; set; } // 1: Thu
            public decimal DebitAccount { get; set; }  // 2: Chi
            public int DocumentType { get; set; }
            public decimal BalanceAmount { get; set; } // Số dư đầu kỳ
        
    }
}
