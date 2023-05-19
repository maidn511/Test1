using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class ExtentionContractModels
    {
        public ExtentionContractModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public int ContractID { get; set; }
        public int AddTime { get; set; }
        public string Note { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedUser { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int DocumentType { get; set; }
        public int TotalRows { get; set; }
        public DateTime? ToDateContract { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
