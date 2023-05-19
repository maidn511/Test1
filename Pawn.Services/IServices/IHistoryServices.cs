using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IHistoryServices
    {
        void AddHistory(HistoryModels objHistory);
        MessageModels AddHistoryMessage(HistoryModels objHistory);
        List<HistoryModels> LoadDataHistory(int contractId, int historyType, int storeId);
    }
}
