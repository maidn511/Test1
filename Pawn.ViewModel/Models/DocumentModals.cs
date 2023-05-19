using Pawn.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Models
{
    public class DocumentModals
    {
        public int Id { get; set; }
        public string DocName { get; set; }
        public int DocType { get; set; }
        public string Description { get; set; }
    }
}
