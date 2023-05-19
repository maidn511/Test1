using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Libraries;
using Pawn.Logger;
using Pawn.ViewModel.Models;

namespace Pawn.Services
{
    public class IncomeAndExpenseServices : IIncomeAndExpenseServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private ICashBookServices _cashBookServices;
        public IncomeAndExpenseServices(IUnitOfWork unitOfWork, IMapper mapper, ICashBookServices cashBookServices)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cashBookServices = cashBookServices;
        }
        public List<IncomeAndExpenseModels> LoadDataIncomeAndExpense(string strKeyword, int intIsActive, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { strKeyword, intIsActive, intPageSize, intPageIndex - 1 };
                var data = _unitOfWork.ExecStoreProdure<IncomeAndExpenseModels>("SP_INCOMEANDEXPENSE_SRH {0}, {1}, {2}, {3}", _params).ToList();
                return data;

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> LoadDataIncomeAndExpense ", ex);
                return new List<IncomeAndExpenseModels>();
            }
        }

        public IEnumerable<IncomeAndExpenseModels> GetAllData(int storeId, int voucherType, int? method, DateTime? fromDate, DateTime? toDate, int currentPage, int pageSize)
        {
            try
            {
                var model = _unitOfWork.IncomeAndExpenseRepository.Filter(x => (fromDate == null || DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(fromDate))
                                                                               && (toDate == null || DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(toDate)) 
                                                                               && x.StoreId == storeId && x.VoucherType == voucherType && !x.IsDeleted
                                                                               && (method == null || x.Method == method))
                .OrderByDescending(m => m.CreatedDate).ToList();
                var data = model.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var result = _mapper.Map<IEnumerable<Tb_IncomeAndExpense>, IEnumerable<IncomeAndExpenseModels>>(data);
                result = result.Select(m => { m.TotalRows = model.Count();return m; });
                return result;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> LoadDataIncomeAndExpense ", ex);
                return new List<IncomeAndExpenseModels>();
            }
        }

        public MessageModels AddIncomeAndExpense(IncomeAndExpenseModels pawnIncomeAndExpense, int type)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thêm mới phiếu {(type > 1 ? "chi" : "thu")} thất bại!"
            };

            try
            {
                var incomeAndExpense = _mapper.Map<IncomeAndExpenseModels, Tb_IncomeAndExpense>(pawnIncomeAndExpense);
                _unitOfWork.IncomeAndExpenseRepository.Insert(incomeAndExpense);
                var result = _unitOfWork.SaveChanges();
                if(result > 0)
                {
                    _cashBookServices.AddCashBook(new CashBookModals {
                        DocumentName = incomeAndExpense.DocumentName,
                        StoreId = incomeAndExpense.StoreId??0,
                        DocumentType = (int)DocumentTypeEnum.ThuChiHoatDong,
                        DocumentDate = incomeAndExpense.DocumentDate ?? DateTime.Now,
                        CreatedDate = incomeAndExpense.CreatedDate??DateTime.Now,
                        CreatedUser = incomeAndExpense.CreatedUser,
                        CreditAccount = (type == 1)?incomeAndExpense.MoneyNumber ?? 0 : 0,
                        DebitAccount = (type == 2) ? incomeAndExpense.MoneyNumber ?? 0 : 0,
                        Customer = incomeAndExpense.Customer,
                        Note = incomeAndExpense.Reason,
                        VoucherType = type
                    });
                    message.Type  = MessageTypeEnum.Success;
                    message.Message = $"Thêm mới phiếu {(type > 1 ? "chi" : "thu")} thành công!";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> AddIncomeAndExpense ", ex);
            }

            return message;
        }

        public MessageModels DeleteIncomeAndExpense(int idIncomeAndExpense, string strUserDelete)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Xóa phiếu thất bại!"
            };
            try
            {
                var incomeAndExpense = _unitOfWork.IncomeAndExpenseRepository.Find(s => s.Id == idIncomeAndExpense);
                if (incomeAndExpense != null)
                {
                    incomeAndExpense.IsDeleted = true;
                    incomeAndExpense.DeletedDate = DateTime.Now;
                    incomeAndExpense.DeletedUser = strUserDelete;
                    var result = _unitOfWork.SaveChanges();
                    if(result > 0)
                    {
                        _cashBookServices.DeleteCashBook(incomeAndExpense.DocumentName,
                            (int)DocumentTypeEnum.ThuChiHoatDong, strUserDelete);
                        message.Type = MessageTypeEnum.Success;
                        message.Message = $"Xóa phiếu {incomeAndExpense.DocumentName} thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> DeleteIncomeAndExpense ", ex);
            }
            return message;
        }

    }
}
