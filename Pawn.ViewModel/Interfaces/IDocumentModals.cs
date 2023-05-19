using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Interfaces
{
    public interface IDocumentModals
    {
        string DocumentName { get; set; }
        int DocumentType { get; set; }
        DateTime DocumentDate { get; set; }
    }
}
