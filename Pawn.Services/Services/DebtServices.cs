using System;
using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.IDataAccess;
using Pawn.ViewModel.Models;
using Pawn.Core.DataModel;
using Pawn.Logger;
using Pawn.Libraries;

namespace Pawn.Services
{

    public class DebtServices : IDebtServices
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        private readonly IHistoryServices _history;
        public DebtServices(IUnitOfWork unitOfWork, IMapper mapper, IHistoryServices history)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _history = history;
        }

        public MessageModels AddDebt(DebtModels debtModels)
        {
            var message = new MessageModels
            {
                Message = $"{(debtModels.IsDebt ? "Ghi nợ" : "Trả nợ")} thất bại!",
                Type = MessageTypeEnum.Error
            };
            try
            {
                var result = -1;
                debtModels.CreatedUser = RDAuthorize.Username;
                var objDebt = _mapper.Map<DebtModels, Tb_Debt>(debtModels);
                _unitOfWork.DebtRepository.Insert(objDebt);
                result = _unitOfWork.SaveChanges();

                if (result > 0)
                {
                    var history = new HistoryModels
                    {
                        Content = message.Message.Replace(" thất bại!", ""),
                        ContractID = debtModels.ContractId,
                        TypeHistory = debtModels.DocumentType,
                        StoreId = RDAuthorize.Store.Id
                    };
                    if (debtModels.IsDebt) history.DebitMoney = debtModels.MoneyNumber;
                    else history.HavingMoney = debtModels.MoneyNumber;
                    _history.AddHistory(history);

                    message.Message = $"{(debtModels.IsDebt ? "Ghi nợ" : "Trả nợ")} thành công!";
                    message.Type = MessageTypeEnum.Success;
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("DebtServices --> AddDebt", ex);
            }
            return message;
        }
    }
}
