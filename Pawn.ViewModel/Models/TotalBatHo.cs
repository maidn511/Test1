using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class TotalBatHo
    {
        public decimal TotalMoneyInSafe { get; set; } // QUỸ TIỀN MẶT
        public decimal TotalMoneyInvestment { get; set; } // TIỀN CHO VAY
        public decimal TotalMoneyDebtor { get; set; } // TIỀN NỢ
        public decimal TotalInterestExpected { get; set; } // LÃI DỰ KIẾN
        public decimal TotalInterestEarned { get; set; } // LÃI ĐÃ THU
    }
}
