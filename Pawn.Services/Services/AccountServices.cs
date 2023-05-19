using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Libraries;
using Pawn.Logger;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawn.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStoreServices _storeService;

        public AccountServices(IUnitOfWork unitOfWork, IMapper mapper, IStoreServices storeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _storeService = storeService;
        }

        public IEnumerable<AccountModels> GetAllData()
        {
            try
            {
                var model = _unitOfWork.AccountRepository.GetAllData();
                return _mapper.Map<IEnumerable<Tb_Account>, IEnumerable<AccountModels>>(model);
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetListAccount ", ex);
                return new List<AccountModels>();
            }
        }

        public List<AccountModels> GetListAccount(string strKeyword, int intIsActive, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { RDAuthorize.IsRoot, RDAuthorize.Username, strKeyword, intIsActive, intPageSize, intPageIndex - 1 };
                var lstData = _unitOfWork.ExecStoreProdure<AccountModels>("SP_ACCOUNT_SRH {0}, {1}, {2}, {3}, {4}, {5}", _params).ToList();
                for (int i = 0; i < lstData.Count; i++)
                {
                    var accId = lstData[i].Id;
                    var store = _unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == accId)
                                                                  .Join(_unitOfWork.StoreRepository.GetAllData(),
                                                                        sa => sa.StoreId,
                                                                        st => st.Id,
                                                                        (sa, st) => st.Name).ToList();
                    if (store != null && store.Any()) lstData[i].StoreName = string.Join(", ", store);
                }
                return lstData;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetListAccount ", ex);
                return new List<AccountModels>();
            }
        }

        public AccountModels LoadDetailAccount(string strUsername)
        {
            try
            {
                var account = _unitOfWork.AccountRepository.Find(s => s.Username == strUsername && (RDAuthorize.IsRoot || RDAuthorize.Username == strUsername || s.CreatedUser == RDAuthorize.Username));
                account.AccountType = _unitOfWork.UserRoleRepository.Find(s => s.AccountId == account.Id)?.RoleId;
                var accoutModel = _mapper.Map<Tb_Account, AccountModels>(account);

                var store = _unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == account.Id)
                                                              .Join(_unitOfWork.StoreRepository.GetAllData(),
                                                                    act => act.StoreId,
                                                                    st => st.Id,
                                                                    (act, st) => st)
                                                              .Select(s => new PawnStoreModels
                                                              {
                                                                  Id = s.Id,
                                                                  Address = s.Address,
                                                                  Name = s.Name,
                                                                  OwnerName = s.OwnerName
                                                              }).ToList();
                accoutModel.Store = store.FirstOrDefault();
                return accoutModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> LoadDetailAccount ", ex);
                return new AccountModels();
            }
        }

        public int AddAccount(AccountModels pawnAccount)
        {
            int result;
            try
            {
                var rs = -1;
                Tb_Account account;
                if (pawnAccount.Id < 1)
                {
                    account = _mapper.Map<AccountModels, Tb_Account>(pawnAccount);
                    _unitOfWork.AccountRepository.Insert(account);
                }
                else
                {
                    account = _unitOfWork.AccountRepository.Find(s => s.Id == pawnAccount.Id);
                    if (account != null)
                    {
                        account.Firstname = pawnAccount.Firstname;
                        account.IsActive = pawnAccount.IsActive;
                        account.IsCms = pawnAccount.IsCms;
                        account.Email = pawnAccount.Email;
                        account.Lastname = pawnAccount.Lastname;
                        account.Phone = pawnAccount.Phone;
                        account.Gender = pawnAccount.Gender;
                        account.Address = pawnAccount.Address;
                        account.Birthday = pawnAccount.Birthday;
                        account.Avatar = pawnAccount.Avatar;
                        if (pawnAccount.IsChangePass)
                        {
                            account.Password = pawnAccount.Password;
                        }
                        _unitOfWork.AccountRepository.Update(account);
                    }
                }
                rs = _unitOfWork.SaveChanges();
                if (rs > 0)
                {
                    if (pawnAccount.Id < 1)
                    {
                        var accountStore = new Tb_Account_Store
                        {
                            AccountId = account.Id,
                            StoreId = pawnAccount.Store?.Id ?? 0,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            IsDeleted = false,
                        };
                        _unitOfWork.StoreAccountRepository.Insert(accountStore);

                        var role = new Tb_UserRole
                        {
                            AccountId = account.Id,
                            RoleId = account.AccountType ?? 0,
                            StoreId = 0
                        };
                        _unitOfWork.UserRoleRepository.Insert(role);
                    }
                    else
                    {
                        var role = _unitOfWork.UserRoleRepository.Find(s => s.AccountId == account.Id);
                        if (role != null)
                        {
                            role.RoleId = pawnAccount.AccountType ?? 0;
                        }
                        var storeUser = _unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == account.Id);
                        if (storeUser.Count() == 1)
                        {
                            var store = _unitOfWork.StoreAccountRepository.Find(s => s.AccountId == account.Id);
                            if (store != null)
                                store.StoreId = pawnAccount.Store?.Id ?? 0;
                        }
                    }
                    _unitOfWork.SaveChanges();
                }
                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> AddAccount ", ex);
                result = (int)Result.Error;
            }

            return result;
        }

        public int DeleteAccount(int idUser, string strUserDelete)
        {
            int result;
            try
            {
                var account = _unitOfWork.AccountRepository.Find(s => s.Id == idUser);
                if (account != null)
                {
                    account.IsDeleted = true;
                    account.DeletedDate = DateTime.Now;
                    account.DeletedUser = strUserDelete;

                    _unitOfWork.SaveChanges();
                }

                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> DeleteAccount ", ex);
                result = (int)Result.Error;
            }
            return result;
        }

        public AccountModels Login(AccountModels pawnAccountModels)
        {
            try
            {
                var objAccount = _unitOfWork.AccountRepository.Filter(s => s.Username == pawnAccountModels.Username
                                                                        && s.Password == pawnAccountModels.Password
                                                                        && s.IsActive && !s.IsDeleted)
                                                             .Select(s => new AccountModels
                                                             {
                                                                 Address = s.Address,
                                                                 Avatar = s.Avatar,
                                                                 Birthday = s.Birthday,
                                                                 CreatedDate = s.CreatedDate,
                                                                 CreatedUser = s.CreatedUser,
                                                                 Email = s.Email,
                                                                 Firstname = s.Firstname,
                                                                 Gender = s.Gender,
                                                                 Id = s.Id,
                                                                 Lastname = s.Lastname,
                                                                 Username = s.Username,
                                                                 Phone = s.Phone
                                                             }).FirstOrDefault();

                objAccount.ListStores = _storeService.GetListStoreByUser(objAccount.Id);
                var objStore = objAccount.ListStores.OrderBy(s => s.Id).FirstOrDefault();
                objAccount.Store = objStore;
                return objAccount;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> Login ", ex);
                return null;
            }
        }

        public List<AccountModels> GetListAccountByStoreId()
        {
            try
            {
                int rootId = RoleEnum.Root;
                var model = _unitOfWork.StoreAccountRepository.Filter(x => x.StoreId == RDAuthorize.Store.Id)
                                                              .Join(_unitOfWork.AccountRepository.GetAllData(), sa => sa.AccountId,
                                                                                                              ac => ac.Id,
                                                                                                              (sa, ac) => new { sa, ac })
                                                              .Join(_unitOfWork.UserRoleRepository.Filter(s => s.RoleId != rootId),
                                                                                                        a => a.ac.Id,
                                                                                                        ur => ur.AccountId,
                                                                                                        (a, ur) => new { a.ac.Id, a.ac.Username })
                                                              .Select(x => new AccountModels
                                                              {
                                                                  Id = x.Id,
                                                                  Username = x.Username
                                                              }).ToList();

                return model;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetListAccount ", ex);
                return new List<AccountModels>();
            }
        }

        public AccountModels GetAccountByUserName(string UserName)
        {
            try
            {
                var model = _unitOfWork.AccountRepository.Find(x => x.Username == UserName);
                var userModel = _mapper.Map<Tb_Account, AccountModels>(model);
                return userModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetAccountByUserName ", ex);
                return new AccountModels();
            }
        }

        public List<AccountModels> GetListAccountByLevel(int userId)
        {
            try
            {
                List<AccountModels> lsResult = new List<AccountModels>();
                // Get level của user cấp quyền
                int StoreID = RDAuthorize.Store.Id;
                var queryLevel = _unitOfWork.UserRoleRepository
                    .Filter(s => s.AccountId == userId)
                    .Join(_unitOfWork.RoleRepository.GetAllData(),
                        userrole => userrole.RoleId,
                        role => role.Id,
                        (userrole, role) => new { role.Level }
                    ).FirstOrDefault().Level;
                int temp = 0;
                int level = int.TryParse(queryLevel.ToString(), out temp) ? int.Parse(queryLevel.ToString()) : -1;
                if (level > -1)
                {
                    var queryAcc = _unitOfWork.UserRoleRepository.GetAllData()
                        .Join(_unitOfWork.RoleRepository.Filter(x => x.Level >= level),
                            us => us.RoleId,
                            r => r.Id,
                            (us, r) => new { us.AccountId }
                        ).Select(x => x.AccountId).Distinct().ToList();


                    var lsUser = _unitOfWork.AccountRepository.Filter(x => queryAcc.Any(y => y.Equals(x.Id)))
                        .Join(_unitOfWork.StoreAccountRepository.Filter(x => x.StoreId == StoreID),
                            a => a.Id,
                            s => s.AccountId,
                            (account, store) => new { account }
                        ).Select(x => x.account).ToList();
                    lsResult = _mapper.Map<List<Tb_Account>, List<AccountModels>>(lsUser);
                }
                return lsResult;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetListAccountLinq ", ex);
                return new List<AccountModels>();
            }
        }

        public MessageModels ChangePass(string oldPass, string newPass)
        {
            var message = new MessageModels
            {
                Message = "Đổi mật khẩu thất bại",
                Type = MessageTypeEnum.Error
            };

            try
            {
                var result = -1;
                var md5 = Encryption.Md5Encryption(oldPass);
                var user = _unitOfWork.AccountRepository.Find(s => s.Username == RDAuthorize.Username && s.Password == md5);
                if (user != null)
                {
                    user.Password = Encryption.Md5Encryption(newPass);
                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedUser = RDAuthorize.Username;

                    result = _unitOfWork.SaveChanges();
                    if (result > 0)
                    {
                        message.Type = MessageTypeEnum.Success;
                        message.Message = "Đổi mật khẩu thành công!";
                    }
                }
                else
                {
                    message = new MessageModels
                    {
                        Message = "Mật khẩu cũ không đúng. Vui lòng thử lại",
                        Type = MessageTypeEnum.Warning
                    };
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> ChangePass ", ex);
            }

            return message;
        }
    }
}
