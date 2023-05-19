using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class TimerModels
    {
        public TimerModels()
        {
            CreatedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public int ContractId { get; set; }
        public DateTime TimerDate { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public int DocumentType { get; set; }
        public int TotalRows { get; set; }

        public string StatusName
        {
            get
            {
                string statusName = "";
                switch (Status)
                {
                    case 0:
                        statusName = "Dừng hẹn giờ";
                        break;
                    case 1:
                        statusName = "Hẹn giờ";
                        break;
                    default:
                        break;
                }
                return statusName;
            }
        }
    }
}
