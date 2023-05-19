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
    public class FileServices : IFileServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FileServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<FileManagementModels> LoadFile(DocumentTypeEnum documentTypeEnum, int contractId)
        {
            try
            {
                var typeId = (int)documentTypeEnum;
                var data = _unitOfWork.FileManagementRepository.Filter(s => s.ContractId == contractId && 
                                                                       s.ContractType == typeId && s.IsDeleted == false)
                                                               .ToList();
                var dataModel = _mapper.Map<List<Tb_FileManagement>, List<FileManagementModels>>(data);
                return dataModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("FileServices --> LoadFile ", ex);
                return new List<FileManagementModels>();
            }
        }

        public FileManagementModels AddFile(FileManagementModels objFile)
        {
            try
            {
                var rs = -1;
                objFile.CreatedUser = RDAuthorize.Username;
                objFile.CreatedDate = DateTime.Now;
                var tbFile = _mapper.Map<FileManagementModels, Tb_FileManagement>(objFile);
                _unitOfWork.FileManagementRepository.Insert(tbFile);
                rs = _unitOfWork.SaveChanges();
                objFile.Id = tbFile.Id;
            }
            catch (Exception ex)
            {
                PawnLog.Error("FileServices --> AddFile ", ex);
            }
            return objFile;
        }

        public MessageModels DeleteFile(int idFile)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Xóa ảnh thất bại!"
            };
            try
            {
                var rs = -1;
                var objFile = _unitOfWork.FileManagementRepository.Find(s => s.Id == idFile);
                if(objFile != null)
                {
                    objFile.IsDeleted = true;
                    objFile.DeletedDate = DateTime.Now;
                    objFile.DeletedUser = RDAuthorize.Username;
                    rs = _unitOfWork.SaveChanges();
                }
                message.Type = rs > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = $"Xóa ảnh {(rs > 0 ? "thành công" : "thất bại")}!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("FileServices --> DeleteFile ", ex);
            }
            return message;
        }
    }
}
