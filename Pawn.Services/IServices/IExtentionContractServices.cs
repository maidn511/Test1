using Pawn.Libraries;
using Pawn.ViewModel.Models;
using System.Collections.Generic;

namespace Pawn.Services
{
    public interface IExtentionContractServices
    {
        List<ExtentionContractModels> LoadDataExtentionContract(int contractId, DocumentTypeEnum documentType);
        MessageModels AddExtentionContract(ExtentionContractModels extentionContractModels);
    }
}
