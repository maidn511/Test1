using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class StoreModels
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsActive { get; set; }
        public decimal MoneyNumber { get; set; }
        public bool? IsDelete { get; set; }
    }
}
