using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class StatusModels
    {
        public StatusModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }

        public string StatusName { get; set; }

        public int Type { get; set; }

        public string DescriptionType { get; set; }

        public bool IsActive { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CreatedUser { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string DeletedUser { get; set; }

        public bool IsDeleted { get; set; }

        public int TotalRows { get; set; }
    }
}
