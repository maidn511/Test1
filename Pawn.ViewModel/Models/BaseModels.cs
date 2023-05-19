using Pawn.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class BaseModels
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedUser { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string DeletedUser { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public int TotalRows { get; set; }
    }
}
