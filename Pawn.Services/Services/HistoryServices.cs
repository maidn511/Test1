using System;
using System.Collections.Generic;
using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Logger;
using Pawn.ViewModel.Models;
using System.Linq;
using Pawn.Libraries;

namespace Pawn.Services
{
    public class HistoryServices : IHistoryServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public HistoryServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<HistoryModels> LoadDataHistory(int contractId, int historyType, int storeId)
        {
            try
            {
                var data = _unitOfWork.HistoryRepository.Filter(s => s.ContractID == contractId && s.StoreId == storeId
                && s.TypeHistory == historyType)
                                                        .Join(_unitOfWork.AccountRepository.GetAllData(),
                                                              hr => hr.CreatedBy,
                                                              ac => ac.Username,
                                                              (hr, ac) => new { hr, ac.Firstname, ac.Lastname })
                                                        .Select(s => new HistoryModels
                                                        {
                                                            Content = s.hr.Content,
                                                            ActionDate = s.hr.ActionDate,
                                                            DebitMoney = s.hr.DebitMoney,
                                                            HavingMoney = s.hr.HavingMoney,
                                                            CreatedBy = s.Firstname + " " + s.Lastname
                                                        }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCustomerServices --> LoadDataCustomer ", ex);
                return new List<HistoryModels>();
            }
        }

        public void AddHistory(HistoryModels objHistory)
        {
            try
            {
                objHistory.CreatedBy = RDAuthorize.Username;
                var historyModel = _mapper.Map<HistoryModels, Tb_History>(objHistory);
                _unitOfWork.HistoryRepository.Insert(historyModel);
                var result = _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnHistoryServices --> AddHistory ", ex);
            }
        }

        public MessageModels AddHistoryMessage(HistoryModels objHistory)
        {
            MessageModels message = new MessageModels
            {
                Message = "Thêm mới lịch sữ nhắc nhỡ thất bại!",
                Type = MessageTypeEnum.Error
            };
            try
            {
                objHistory.CreatedBy = RDAuthorize.Username;
                var historyModel = _mapper.Map<HistoryModels, Tb_History>(objHistory);
                _unitOfWork.HistoryRepository.Insert(historyModel);
                var result = _unitOfWork.SaveChanges();
                if(result > 0)
                {
                    message.Message = "Thêm mới lịch sữ nhắc nhỡ thành công!.";
                    message.Type = MessageTypeEnum.Success;
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnHistoryServices --> AddHistory ", ex);
            }
            return message;
        }
    }
}
