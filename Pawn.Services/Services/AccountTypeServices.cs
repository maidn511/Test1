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
    public class AccountTypeServices : IAccountTypeServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AccountTypeServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<RoleModels> LoadDataAccountType(string strKeyword, int intIsActive, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { strKeyword, intIsActive, intPageSize, intPageIndex - 1 };
                var lstData = _unitOfWork.ExecStoreProdure<RoleModels>("SP_ROLE_SRH {0}, {1}, {2}, {3}", _params).ToList();
                return lstData;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetListAccount ", ex);
                return new List<RoleModels>();
            }
        }
    }
}
