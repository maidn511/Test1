using Pawn.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class ParametersModels
    {
        public int? customerId { get; set; }
        public string customerName { get; set; }
        public int? userId { get; set; }
        public string userName { get; set; }
        public string fromDateString { get; set; }
        public string toDateString { get; set; }

        public DateTime? fromDate
        {
            get
            {
                DateTime? dateFormat = null;
                if (!string.IsNullOrEmpty(fromDateString))
                    dateFormat = fromDateString.ToDateTimeNull(Constants.DateFormat);
                return dateFormat;
            }
        }
        public DateTime? toDate {
            get
            {
                DateTime? dateFormat = null;
                if (!string.IsNullOrEmpty(toDateString))
                    dateFormat = toDateString.ToDateTimeNull(Constants.DateFormat);
                return dateFormat;
            }
        }
        public int? timePawn { get; set; }
        public string documentName { get; set; }
        public int? documentType { get; set; }
        public int? documentStatus { get; set; }
        public int? voucherType { get; set; }

        public string Code { get; set; }
        public string Keyword { get; set; }
        public string columnsort { get; set; }
        public string sorttype { get; set; }
        public int StatusContractId { get; set; }
        public int? StaffManagerId { get; set; }
        public int? Method { get; set; }
        public List<int> Notes { get; set; } 
    }
}
