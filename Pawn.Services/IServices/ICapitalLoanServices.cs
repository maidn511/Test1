using Pawn.Libraries;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface ICapitalLoanServices
    {
        MessageModels AddAddWithDrawCapital(CapitalLoanModels objCapitalLoanModel);
        List<CapitalLoanModels> LoadDataCapitalLoan(int capitalId, DocumentTypeEnum documentType);
        MessageModels DeleteCapitalLoan(int id, DocumentTypeEnum documentType);
    }
}
