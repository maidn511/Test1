using AutoMapper;
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
    public class DocumentServices : IDocumentServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DocumentServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<DocumentModals> GetAllData()
        {
            try
            {
                var model = _unitOfWork.DocumentRepository.GetAllData();
                return _mapper.Map<IEnumerable<Tb_Document>, IEnumerable<DocumentModals>>(model);
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> GetListAccount ", ex);
                return new List<DocumentModals>();
            }
        }
    }
}
