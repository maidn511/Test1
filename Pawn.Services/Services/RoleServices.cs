using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Libraries;
using Pawn.Logger;
using Pawn.ViewModel.Models;

namespace Pawn.Services
{
    public class RoleServices : IRoleServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public RoleServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public List<RoleModels> LoadDataRole(string strKeyword, int intIsActive, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { strKeyword, intIsActive, intPageSize, intPageIndex - 1 };
                var data = _unitOfWork.ExecStoreProdure<RoleModels>("SP_ROLE_SRH {0}, {1}, {2}, {3}", _params).ToList();
                return data;

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> LoadDataRole ", ex);
                return new List<RoleModels>();
            }
        }

        public RoleModels LoadDetailRole(int intRoleId)
        {
            try
            {
                var role = _unitOfWork.RoleRepository.Find(s => s.Id == intRoleId);
                var roleModel = _mapper.Map<Tb_Role, RoleModels>(role);
                return roleModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> LoadDetailRole ", ex);
                return new RoleModels();
            }
        }

        public int AddRole(RoleModels pawnRole)
        {
            int result;
            try
            {
                if (pawnRole.Id < 1)
                {
                    var Role = _mapper.Map<RoleModels, Tb_Role>(pawnRole);
                    _unitOfWork.RoleRepository.Insert(Role);
                }
                else
                {
                    var role = _unitOfWork.RoleRepository.Find(s => s.Id == pawnRole.Id);
                    if (role != null)
                    {
                        role.RoleName = pawnRole.RoleName;
                        role.IsActive = pawnRole.IsActive;
                        role.UpdatedDate = pawnRole.UpdatedDate;
                        role.UpdatedUser = pawnRole.UpdatedUser;
                    }
                }
                _unitOfWork.SaveChanges();
                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> AddRole ", ex);
                result = (int)Result.Error;
            }

            return result;
        }

        public int DeleteRole(int idRole, string strUserDelete)
        {
            int result;
            try
            {
                var role = _unitOfWork.RoleRepository.Find(s => s.Id == idRole);
                if (role != null)
                {   
                    role.IsDeleted = true;
                    role.DeletedDate = DateTime.Now;
                    role.DeletedUser = strUserDelete;

                    _unitOfWork.SaveChanges();
                }

                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> DeleteRole ", ex);
                result = (int)Result.Error;
            }
            return result;
        }

        public List<int> GetLstRoleIdByUserId(long userId)
        {
            try
            {
                var role = _unitOfWork.UserRoleRepository.Filter(s => s.AccountId == userId).Select(s => s.RoleId).ToList();
               
                return role;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> GetLstRoleIdByUserId ", ex);
                return new List<int>();
            }
        }

        public List<RoleModels> LoadListRoleAdmin()
        {

            return null;
        }

        public List<RoleModels> LoadRoleLevelByUser(int userID)
        {
            try
            {
                List<RoleModels> lsResult = new List<RoleModels>();
                var queryLevel = _unitOfWork.UserRoleRepository
                    .Filter(s => s.AccountId == userID)
                    .Join(_unitOfWork.RoleRepository.GetAllData(),
                        userrole => userrole.RoleId,
                        role => role.Id,
                        (userrole, role) => new { role.Level }
                    ).FirstOrDefault().Level;
                int temp = 0;
                int level = int.TryParse(queryLevel.ToString(), out temp) ? int.Parse(queryLevel.ToString()) : -1;
                if(level > -1)
                {
                    var queryRole = _unitOfWork.RoleRepository.Filter(x => x.Level >= level).ToList();
                    lsResult = _mapper.Map<List<Tb_Role>, List<RoleModels>>(queryRole);
                }
                return lsResult;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> LoadRoleLevelByUser ", ex);
                return new List<RoleModels>();
            }
        }

        public int AddRoleV2(UserRoleModels userRole)
        {
            int result;
            try
            {
                var role = _unitOfWork.UserRoleRepository.Find(s => s.AccountId == userRole.AccountId);
                if (role != null)
                {
                    role.RoleId = userRole.RoleId;
                }
                _unitOfWork.SaveChanges();
                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnRoleServices --> AddRoleV2 ", ex);
                result = (int)Result.Error;
            }

            return result;
        }
    }
}
