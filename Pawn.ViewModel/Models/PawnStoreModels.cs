using System;

namespace Pawn.ViewModel.Models
{
    public class PawnStoreModels
    {
        public PawnStoreModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedUser { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int TotalRows { get; set; }

        public decimal MoneyNumber { get; set; }
    }

}
