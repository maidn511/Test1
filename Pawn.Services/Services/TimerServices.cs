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
    public class TimerServices : ITimerServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public TimerServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<TimerModels> LoadDataTimer(int contractId, int documentType)
        {
            try
            {
                var data = _unitOfWork.TimerRepository.Filter(s => s.ContractId == contractId && s.DocumentType == documentType)
                                                      .Select(s => new TimerModels
                                                      {
                                                          ContractId = s.ContractId,
                                                          CreatedDate = s.CreatedDate,
                                                          DocumentType = s.DocumentType,
                                                          Id = s.Id,
                                                          Note = s.Note,
                                                          Status = s.Status,
                                                          TimerDate = s.TimerDate
                                                      }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("TimerServices --> LoadDataTimer ", ex);
                return new List<TimerModels>();
            }
        }

        public MessageModels AddTimer(TimerModels timerModels)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"{timerModels.StatusName} thất bại!"
            };

            try
            {
                var result = -1;
                timerModels.CreatedUser = RDAuthorize.Username;
                var objTimer = _mapper.Map<TimerModels, Tb_Timer>(timerModels);
                _unitOfWork.TimerRepository.Insert(objTimer);
                result = _unitOfWork.SaveChanges();

                if(result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    message.Message = $"{timerModels.StatusName} thành công";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("TimerServices --> AddTimer ", ex);
            }

            return message;
        }
    }
}
