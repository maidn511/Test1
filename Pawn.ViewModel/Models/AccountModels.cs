using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class AccountModels
    {
        public AccountModels()
        {
            ListRoles = new List<RoleModels>();
            CreatedDate = DateTime.Now;
        }
        public int Id { get; set; }
        public int? AccountType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public bool Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsCms { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedUser { get; set; }

        public string AccountTypeName { get; set; }
        public int TotalRows { get; set; }
        public List<RoleModels> ListRoles { get; set; }
        public IEnumerable<PawnStoreModels> ListStores { get; set; }
        public PawnStoreModels Store { get; set; }
        public List<int> ListRole { get; set; }

        public string StoreName { get; set; }

        public bool IsRoot { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsUserStore { get; set; }
        public bool IsUser { get; set; }

        public bool IsChangePass { get; set; }
    }
}
