using Pawn.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class CapitalPayDayModels
    {
        public CapitalPayDayModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public int CapitalId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal MoneyNumber { get; set; }
        public decimal? MoneyOrther { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ToDateString { get; set; }
        public string FromDateString { get; set; }
        public int NumberOfDay { get; set; }
        public DateTime FromDatePost
        {
            get
            {
                DateTime dateFormat = DateTime.Now;
                if (!string.IsNullOrEmpty(FromDateString))
                    dateFormat = FromDateString.ToDateTime(Constants.DateFormat);
                return dateFormat;
            }
        }

        public DateTime ToDatePost
        {
            get
            {
                DateTime dateFormat = DateTime.Now;
                if (!string.IsNullOrEmpty(ToDateString))
                    dateFormat = ToDateString.ToDateTime(Constants.DateFormat);
                return dateFormat;
            }
        }
        public bool IsPaid { get; set; }
        public int TotalRows { get; set; }
        public string CustomerName { get; set; }
    }

}
