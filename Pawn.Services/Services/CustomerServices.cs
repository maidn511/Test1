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
    public class CustomerServices : ICustomerServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public CustomerServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<CustomerModels> LoadDataCustomer(string strKeyword, int intStatusId, int intPageSize, int intPageIndex, int storeId = -1)
        {
            try
            {
                if (storeId == -1)
                    storeId = RDAuthorize.Store.Id;

                var _params = new object[] { storeId, strKeyword, intStatusId, intPageSize, intPageIndex - 1 };
                var data = _unitOfWork.ExecStoreProdure<CustomerModels>("SP_CUSTOMER_SRH {0}, {1}, {2}, {3}, {4}", _params).ToList();
                return data;

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCustomerServices --> LoadDataCustomer ", ex);
                return new List<CustomerModels>();
            }
        }

        public int AddCustomer(CustomerModels pawnCustomer)
        {
            int result;
            try
            {
                if (pawnCustomer.Id < 1)
                {
                    var customer = _mapper.Map<CustomerModels, Tb_Customer>(pawnCustomer);
                    _unitOfWork.CustomerRepository.Insert(customer);
                }
                else
                {
                    var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == pawnCustomer.Id);
                    if (customer != null)
                    {
                        customer.Fullname = pawnCustomer.Fullname;
                        customer.Phone = pawnCustomer.Phone;
                        customer.Address = pawnCustomer.Address;
                        customer.StoreId = pawnCustomer.StoreId;
                        customer.IdentityCard = pawnCustomer.IdentityCard;
                        customer.UpdatedDate = pawnCustomer.UpdatedDate;
                        customer.UpdatedUser = pawnCustomer.UpdatedUser;
                        customer.IsActive = pawnCustomer.IsActive;
                    }
                }
                _unitOfWork.SaveChanges();
                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCustomerServices --> AddCustomer ", ex);
                result = (int)Result.Error;
            }

            return result;
        }

        public CustomerModels LoadDetailCustomer(int? intCustomerId)
        {
            try
            {
                var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == intCustomerId);
                var customerModel = _mapper.Map<Tb_Customer, CustomerModels>(customer);
                return customerModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCustomerServices --> LoadDetailCustomer ", ex);
                return new CustomerModels();
            }
        }

        public IEnumerable<CustomerModels> GetCustomerByStoreId(int? storeId)
        {
            try
            {
                var customer = _unitOfWork.CustomerRepository.Filter(s => s.StoreId == storeId).ToList();
                var customerModel = _mapper.Map<IEnumerable<Tb_Customer>, IEnumerable<CustomerModels>>(customer);
                return customerModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCustomerServices --> LoadDetailCustomer ", ex);
                return new List<CustomerModels>();
            }
        }

        public int DeleteCustomer(int idCustomer, string strUserDelete)
        {
            int result;
            try
            {
                var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == idCustomer);
                if (customer != null)
                {
                    customer.IsDeleted = true;
                    customer.DeletedDate = DateTime.Now;
                    customer.DeletedUser = strUserDelete;

                    _unitOfWork.SaveChanges();
                }

                result = (int)Result.Success;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCustomerServices --> DeleteCustomer ", ex);
                result = (int)Result.Error;
            }
            return result;
        }
    }
}
