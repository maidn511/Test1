using Pawn.ViewModel.Interfaces;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class IncomeAndExpenseModels : BaseModels,IDocumentModals
    {
        public string VoucherCode { get; set; }

        public int VoucherType { get; set; }

        public string Customer { get; set; }

        public decimal MoneyNumber { get; set; }

        public int Method { get; set; }
        public string MethodString { get; set; }

        public string Reason { get; set; }

        public string DocumentName { get; set; }

        public int DocumentType { get; set; }

        public DateTime DocumentDate { get; set; }
        public int StoreId { get; set; }

    }
}
