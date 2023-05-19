using System;
namespace Pawn.ViewModel.Models
{
    public class CustomerModels
    {
        public CustomerModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public int? StoreId { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int? StatusId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedUser { get; set; }
        public bool IsActive { get; set; }
        public string IdentityCard { get; set; }
        public string ICCreateDate { get; set; }
        public string ICCreatePlace { get; set; }
        public int TotalRows { get; set; }
    }
}
