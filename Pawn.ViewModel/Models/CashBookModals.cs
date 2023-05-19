using Pawn.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class CashBookModals : BaseModels, IDocumentModals
    {
        public string DocumentName { get; set; }
        public int DocumentType { get; set; } //1: Vai Lãi, 2:Bát Họ, 3: Nguồn Vốn....
        public string DocumentTypeString { get; set; } //1: Vai Lãi, 2:Bát Họ, 3: Nguồn Vốn....
        public DateTime DocumentDate { get; set; }
        public int VoucherType { get; set; } // 1: Thu, 2: Chi
        public string Customer { get; set; }
        public decimal DebitAccount { get; set; }
        public decimal CreditAccount { get; set; }
        public string Note { get; set; }
        public int? NoteId { get; set; }
        public string FullName { get; set; }
        public int StoreId { get; set; }
        public int CustomerId { get; set; }

        public int? ContractId { get; set; }
        public decimal TotalCreditAccount { get; set; }
        public decimal TotalDebitAccount { get; set; }
    }
}
