using AutoMapper;
using Pawn.Core.IDataAccess;
using Pawn.Logger;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public class StatusServices : IStatusServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public StatusServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public List<StatusModels> LoadDataStatus(int intTypeId, string strKeyword, int intIsActive, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { intTypeId, strKeyword, intIsActive, intPageSize, intPageIndex - 1 };
                var data = _unitOfWork.ExecStoreProdure<StatusModels>("SP_STATUS_SRH {0}, {1}, {2}, {3}, {4}", _params).ToList();
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStatusServices --> LoadDataBallotType ", ex);
                return new List<StatusModels>();
            }
        }
    }
}
