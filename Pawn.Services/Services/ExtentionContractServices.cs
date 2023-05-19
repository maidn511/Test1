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
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public class ExtentionContractServices : IExtentionContractServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExtentionContractServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<ExtentionContractModels> LoadDataExtentionContract(int contractId, DocumentTypeEnum documentType)
        {
            var listExtentionContract = new List<ExtentionContractModels>();
            try
            {
                var docType = (int)documentType;
                var data = _unitOfWork.ExtentionContractRepository.Filter(s => s.ContractID == contractId && s.DocumentType == docType);
                listExtentionContract = _mapper.Map<IQueryable<Tb_ExtentionContract>, List<ExtentionContractModels>>(data);
            }
            catch (Exception ex)
            {
                PawnLog.Error("ExtentionContractServices --> LoadDataExtentionContract ", ex);
            }
            return listExtentionContract;
        }


        public MessageModels AddExtentionContract(ExtentionContractModels extentionContractModels)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Gian hạn thất bại"
            };

            try
            {
                extentionContractModels.CreatedUser = RDAuthorize.Username;
                var objExtentionContract = _mapper.Map<ExtentionContractModels, Tb_ExtentionContract>(extentionContractModels);
                _unitOfWork.ExtentionContractRepository.Insert(objExtentionContract);
                var result = _unitOfWork.SaveChanges();
                if(result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    message.Message = "Gia hạn thành công";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("ExtentionContractServices --> AddExtentionContract ", ex);
            }

            return message;
        }
    }
}
