using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class RoleModels
    {
        public int Id { get; set; }

        public string RoleName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string DeletedUser { get; set; }

        public DateTime? DeletedDate { get; set; }

        public bool IsDeleted { get; set; }

        public int TotalRows { get; set; }

        public int Level { get; set; }
    }
}
