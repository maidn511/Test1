using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class MenuModels
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public string MenuName { get; set; }

        public string Icon { get; set; }

        public string Url { get; set; }

        public int RoleId { get; set; }

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; }

        public bool IsCms { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedUser { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string DeletedUser { get; set; }

        public string RoleName { get; set; }
        public int TotalRows { get; set; }

        public string Action { get; set; }
        public string Controller { get; set; }
        public string Description { get; set; }

        public bool IsShow { get; set; }
    }
}
