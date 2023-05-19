using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Libraries;
using Pawn.Logger;
using Pawn.ViewModel.Models;

namespace Pawn.Services
{
    public class StoreServices : IStoreServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly ICapitalServices _capital;
        public StoreServices(IUnitOfWork unitOfWork, IMapper mapper, ICapitalServices capital)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _capital = capital;
        }
        public List<PawnStoreModels> LoadDataStore(string strKeyword, int intIsActive, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { RDAuthorize.IsRoot, RDAuthorize.UserId, strKeyword, intIsActive, intPageSize, intPageIndex - 1 };
                var data = _unitOfWork.ExecStoreProdure<PawnStoreModels>("SP_STORE_SRH {0}, {1}, {2}, {3}, {4}, {5}", _params).ToList();
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStoreServices --> LoadDataStore ", ex);
                return new List<PawnStoreModels>();
            }
        }

        public List<PawnStoreModels> LoadAllStore()
        {
            var lstStore = _unitOfWork.StoreRepository.Filter(s => s.IsDeleted == false);
            var lstData = _mapper.Map<IQueryable<Tb_Store>, List<PawnStoreModels>>(lstStore);
            return lstData;
        }

        public PawnStoreModels LoadDetailStore(int intStoreId)
        {
            try
            {
                Tb_Store store;
                if (RDAuthorize.IsRoot)
                {
                    store = _unitOfWork.StoreRepository.Find(s => s.Id == intStoreId);
                }
                else
                {
                    store = _unitOfWork.StoreRepository.Filter(s => s.Id == intStoreId)
                                                      .Join(_unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == RDAuthorize.UserId),
                                                            st => st.Id,
                                                            sa => sa.StoreId,
                                                            (st, sa) => st)
                                                      .FirstOrDefault();
                }
                if (store == null) return null;
                var storeModel = _mapper.Map<Tb_Store, PawnStoreModels>(store);
                storeModel.MoneyNumber = _unitOfWork.CapitalRepository.Filter(s => s.StoreId == intStoreId).ToList().Sum(s => s.MoneyNumber);
                return storeModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStoreServices --> LoadDetailStore ", ex);
                return new PawnStoreModels();
            }
        }

        public MessageModels AddStore(PawnStoreModels pawnStore)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Thêm cửa hàng thất bại"
            };
            var result = -1;
            Tb_Store store;
            try
            {
                if (pawnStore.Id < 1)
                {
                    if (_unitOfWork.StoreRepository.Filter(s => s.Name.ToUpper() == pawnStore.Name.ToUpper()).Any())
                    {
                        message.Message = $"Tên cửa hàng: <b>{pawnStore.Name}</b> đã tồn tại!";
                        return message;
                    }
                    store = _mapper.Map<PawnStoreModels, Tb_Store>(pawnStore);
                    _unitOfWork.StoreRepository.Insert(store);
                }
                else
                {
                    store = _unitOfWork.StoreRepository.Find(s => s.Id == pawnStore.Id);
                    if (store != null)
                    {
                        store.Name = pawnStore.Name;
                        store.OwnerName = pawnStore.OwnerName;
                        store.Phone = pawnStore.Phone;
                        store.IsActive = pawnStore.IsActive;
                        store.Address = pawnStore.Address;
                        store.UpdatedDate = pawnStore.UpdatedDate;
                        store.UpdatedUser = pawnStore.UpdatedUser;
                    }
                }
                result = _unitOfWork.SaveChanges();
                var storeId = store.Id;
                if (result > 0)
                {
                    if (pawnStore.Id < 1)
                    {
                        var storeAccount = new Tb_Account_Store
                        {
                            StoreId = storeId,
                            AccountId = RDAuthorize.UserId,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            IsDeleted = false
                        };
                        _unitOfWork.StoreAccountRepository.Insert(storeAccount);

                        var mess = _capital.AddCapital(new CapitalModels
                        {
                            CreatedUser = RDAuthorize.Username,
                            CustomerName = "Vốn khởi tạo",
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.NguonVon,
                            IsActive = true,
                            IsSystem = false,
                            Note = "Thêm vốn tự động khi tạo cửa hàng",
                            MoneyNumber = pawnStore.MoneyNumber,
                            StoreId = storeId,
                            Method = (int)CapitalMethodEnum.VonDauTu
                        });
                    }
                }

                if (result > 0)
                {
                    message = new MessageModels
                    {
                        Type = MessageTypeEnum.Success,
                        Message = "Thêm cửa hàng thành công"
                    };
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStoreServices --> AddStore ", ex);
            }

            return message;
        }

        public int DeleteStore(int idStore, string strUserDelete)
        {
            int result;
            try
            {
                var Store = _unitOfWork.StoreRepository.Find(s => s.Id == idStore);
                if (Store != null)
                {
                    Store.IsDeleted = true;
                    Store.DeletedDate = DateTime.Now;
                    Store.DeletedUser = strUserDelete;

                    _unitOfWork.SaveChanges();
                }

                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStoreServices --> DeleteStore ", ex);
                result = (int)Result.Error;
            }
            return result;
        }

        public PawnStoreModels GetStoreByDefault(int userId)
        {
            try
            {
                var storeAccountList = _unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == userId);
                var storeList = _unitOfWork.StoreRepository.GetAllData();
                var storeModel = storeList.Join(storeAccountList,
                 m => m.Id,
                 n => n.StoreId,
                 (m, n) => new { m }).Select(m => m.m).FirstOrDefault();
                var model = _mapper.Map<Tb_Store, PawnStoreModels>(storeModel);
                return model;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStoreServices --> GetStoreById ", ex);
                return new PawnStoreModels();
            }
        }

        public IEnumerable<PawnStoreModels> GetListStoreByUser(int userId)
        {
            try
            {
                var storeAccountList = _unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == userId && (s.IsDeleted ?? false) == false);
                var storeList = _unitOfWork.StoreRepository.GetAllData();
                var storeModel = storeList.Join(storeAccountList,
                    m => m.Id,
                    n => n.StoreId,
                    (m, n) => new { m }).Select(m => m.m).ToList();
                var store = _mapper.Map<IEnumerable<Tb_Store>, IEnumerable<PawnStoreModels>>(storeModel);
                return store;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnStoreServices --> GetStoreById ", ex);
                return null;
            }
        }
    }
}
