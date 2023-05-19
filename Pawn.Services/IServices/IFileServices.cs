using Pawn.Libraries;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IFileServices
    {
        List<FileManagementModels> LoadFile(DocumentTypeEnum documentTypeEnum, int contractId);
        FileManagementModels AddFile(FileManagementModels objFile);
        MessageModels DeleteFile(int idFile);
    }
}
