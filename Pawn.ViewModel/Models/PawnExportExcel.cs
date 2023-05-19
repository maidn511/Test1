using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class PawnExportExcel
    {
        public string Code { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public double TotalMoney { get; set; }
        public double InterestRate { get; set; }
        public string InterestRateString { get; set; }
        public string PawnDate { get; set; }
        public string ToDate { get; set; }
        public string Note { get; set; }
        public string DatePaid { get; set; }
        public decimal MoneyPaid { get; set; }
        public string NextDate { get; set; }
    }
}
