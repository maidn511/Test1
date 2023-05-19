using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Libraries;
using Pawn.Logger;
using Pawn.ViewModel.Models;

namespace Pawn.Services
{
    public class CapitalLoanServices : ICapitalLoanServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly IHistoryServices _history;
        public CapitalLoanServices(IUnitOfWork unitOfWork, IMapper mapper, IHistoryServices history)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _history = history;
        }

        public MessageModels AddAddWithDrawCapital(CapitalLoanModels objCapitalLoanModel)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = !objCapitalLoanModel.IsLoan ? "Rút bớt gốc thất bại!" : "Vay thêm vốn thất bại"
            };

            try
            {
                objCapitalLoanModel.LoadDate = objCapitalLoanModel.LoadDatePost;
                objCapitalLoanModel.CreatedUser = RDAuthorize.Username;
                var capitalLoanModel = _mapper.Map<CapitalLoanModels, Tb_Capital_Loan>(objCapitalLoanModel);
                _unitOfWork.CapitalLoanRepository.Insert(capitalLoanModel);
                var result = _unitOfWork.SaveChanges();
                if (result > 0)
                {
                    var history = new HistoryModels
                    {
                        Content = !objCapitalLoanModel.IsLoan ? "Trả gốc" : "Vay thêm",
                        ContractID = objCapitalLoanModel.CapitalId,
                        TypeHistory = objCapitalLoanModel.DocumentType,
                        StoreId = RDAuthorize.Store.Id
                    };
                    if (objCapitalLoanModel.IsLoan)
                        history.HavingMoney = objCapitalLoanModel.MoneyNumber;
                    else history.DebitMoney = objCapitalLoanModel.MoneyNumber;
                    _history.AddHistory(history);
                }
                message.Type = result > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = !objCapitalLoanModel.IsLoan ? "Rút bớt gốc thành công!" : "Vay thêm vốn thành công";
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalLoanServices --> AddAddWithDrawCapital ", ex);
            }
            return message;
        }

        public List<CapitalLoanModels> LoadDataCapitalLoan(int capitalId, DocumentTypeEnum documentType)
        {
            try
            {
                int docType = (int)documentType;
                var data = _unitOfWork.CapitalLoanRepository.Filter(s => s.CapitalId == capitalId && s.DocumentType == docType)
                                                            .Select(s => new CapitalLoanModels
                                                            {
                                                                LoadDate = s.LoadDate,
                                                                MoneyNumber = s.MoneyNumber,
                                                                Note = s.Note,
                                                                Id = s.Id,
                                                                IsLoan = s.IsLoan
                                                            }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalLoanServices --> LoadDataCapitalLoan ", ex);
                return new List<CapitalLoanModels>();
            }
        }

        public MessageModels DeleteCapitalLoan(int id, DocumentTypeEnum documentType)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Xóa thất bại"
            };

            try
            {
                int docType = (int)documentType;
                var result = -1;
                var objCapitalLoan = _unitOfWork.CapitalLoanRepository.Find(s => s.Id == id && s.DocumentType == docType);
                if(objCapitalLoan != null)
                {
                    _unitOfWork.CapitalLoanRepository.Delete(objCapitalLoan);
                    result = _unitOfWork.SaveChanges();
                }
                if(result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    message.Message = "Xóa thành công";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalLoanServices --> DeleteCapitalLoan ", ex);
            }

            return message;
        }
    }
}
