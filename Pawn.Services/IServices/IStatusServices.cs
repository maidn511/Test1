using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IStatusServices
    {
        List<StatusModels> LoadDataStatus(int intTypeId, string strKeyword, int intIsActive, int intPageSize, int intPageIndex);
    }
}
