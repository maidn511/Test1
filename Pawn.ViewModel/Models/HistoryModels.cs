using System;

namespace Pawn.ViewModel.Models
{
    public class HistoryModels
    {
        public HistoryModels()
        {
            CreatedDate = DateTime.Now;
            ActionDate = DateTime.Now;
        }

        public int Id { get; set; }
        public int? ContractID { get; set; }
        public int? TypeHistory { get; set; }
        public Nullable<DateTime> ActionDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public decimal? DebitMoney { get; set; }
        public decimal? HavingMoney { get; set; }
        public string Content { get; set; }
        public int TotalRows { get; set; }

        public int StoreId { get; set; }
    }
}
