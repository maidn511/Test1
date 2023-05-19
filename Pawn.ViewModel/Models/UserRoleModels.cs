using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class UserRoleModels
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public int? StoreId { get; set; }

        public int RoleId { get; set; }
    }
}
