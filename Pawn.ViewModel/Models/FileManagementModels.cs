using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class FileManagementModels
    {
        public FileManagementModels()
        {
            CreatedDate = DateTime.Now;
        }
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int ContractType { get; set; }
        public string FileName { get; set; }
        public string FileGuild { get; set; }
        public string Ext { get; set; }
        public int? FileSize { get; set; }
        public int? FileType { get; set; }
        public string Url { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedUser { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int TotalRows { get; set; }
    }
}
