using Pawn.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class CapitalLoanModels
    {
        public CapitalLoanModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public int CapitalId { get; set; }
        public DateTime LoadDate { get; set; }
        public decimal MoneyNumber { get; set; }
        public string Note { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsLoan { get; set; }
        public int? DocumentType { get; set; }
        public int TotalRows { get; set; }

        public string LoadDateString { get; set; }
        public DateTime LoadDatePost
        {
            get
            {
                return LoadDateString.ToDateTime(Constants.DateFormat);
            }
        }
    }
}
