using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class DebtModels
    {
        public DebtModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public decimal MoneyNumber { get; set; }
        public int ContractId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public bool IsDebt { get; set; }
        public int DocumentType { get; set; }
        public int TotalRows { get; set; }
    }
}
