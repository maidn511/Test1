using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
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
    public class CashBookServices : ICashBookServices
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public CashBookServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public IEnumerable<CashBookSummaryModels> GetSummaryData(int storeId, DateTime? fromDate, DateTime? toDate, string userName = "")
        {
            try
            {
                var _params = new object[] { fromDate, toDate, userName, storeId };
                var data = _unitOfWork.ExecStoreProdure<CashBookSummaryModels>("SP_CASHBOOK_SUMMARY {0}, {1}, {2}, {3}", _params).ToList();
                return data;

            }
            catch (Exception ex)
            {
                PawnLog.Error("CashBookServices --> GetSummaryData ", ex);
                return new List<CashBookSummaryModels>();
            }
        }

        public IEnumerable<CashBookModals> GetAllData(int storeId, DateTime? fromDate, DateTime? toDate,
            int currentPage, int pageSize, string userName, int? docType, List<int> notes)
        {
            try
            {
                var model = _unitOfWork.CashBookRepository.Filter(x =>
                (fromDate == null || (fromDate != null && DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(fromDate))) && (toDate == null || (toDate != null && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(toDate)))
                && x.StoreId == storeId
                && (String.IsNullOrEmpty(userName) || x.CreatedUser == userName)
                && (docType == null || x.DocumentType == docType)
                && !x.IsDeleted).AsEnumerable().Where(c => notes == null || notes.Count == 0 || notes.Any(m => m == c.NoteId))
                .OrderByDescending(m => m.CreatedDate).ToList();

                var data = model.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var result = _mapper.Map<IEnumerable<Tb_CashBook>, IEnumerable<CashBookModals>>(data);
                result = result.Join(_unitOfWork.DocumentRepository.GetAllData()
                    , m => m.DocumentType
                    , n => n.DocType
                    , (m, n) => new { m, n }).Select(x => { x.m.DocumentTypeString = x.n.Description; return x; })
                    .Select(x => x.m).ToList();
                result.Join(_unitOfWork.AccountRepository.GetAllData(),
                    m => m.CreatedUser,
                    n => n.Username,
                    (m, n) => new { m, n }).Select(x => { x.m.FullName = x.n.Firstname + " " + x.n.Lastname; return x; })
                    .Select(x => x.m).ToList();
                result = result.Select(m => { m.TotalRows = model.Count(); return m; });
                if (model != null && result != null && result.Any())
                {
                    result.ElementAt(0).TotalCreditAccount = model.Sum(s => s.CreditAccount) ?? 0;
                    result.ElementAt(0).TotalDebitAccount = model.Sum(s => s.DebitAccount) ?? 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                PawnLog.Error("CashBookServices --> GetAllData ", ex);
                return new List<CashBookModals>();
            }
        }

        public int AddCashBook(CashBookModals model)
        {
            int result = 0;
            try
            {
                var data = _mapper.Map<CashBookModals, Tb_CashBook>(model);
                data.CreatedDate = DateTime.Now;
                data.CreatedUser = RDAuthorize.Username;
                _unitOfWork.CashBookRepository.Insert(data);
                result = _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                PawnLog.Error("CashBookServices --> AddCashBook ", ex);
            }
            return result;  // isSuccess > 0: isFail
        }

        public int UpdateCashBook(CashBookModals model)
        {
            int result = 0;
            try
            {
                var data = _mapper.Map<CashBookModals, Tb_CashBook>(model);
                _unitOfWork.CashBookRepository.Update(data);
                result = _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                PawnLog.Error("CashBookServices --> UpdateCashBook ", ex);
            }
            return result;  // isSuccess > 0: isFail
        }

        public int DeleteCashBook(string documentName, int documentType, string userName)
        {
            int result = 0;
            try
            {
                var data = _unitOfWork.CashBookRepository.Find(s => s.DocumentName == documentName
                && s.DocumentType == documentType);
                if (data != null)
                {
                    data.IsDeleted = true;
                    data.DeletedDate = DateTime.Now;
                    data.DeletedUser = userName;
                    result = _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("CashBookServices --> DeleteCashBook ", ex);
            }
            return result;
        }

        public MemoryStream ExportExcel(int storeId, DateTime? fromDate, DateTime? toDate,
           string userName, int? docType, List<int> notes)
        {
            MemoryStream stream;
            var excelPackage = new ExcelPackages("Sổ quỹ");
            try
            {
                var dt = new DataTable();
                var model = _unitOfWork.CashBookRepository.Filter(x =>
                                                                (fromDate == null || (fromDate != null && DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(fromDate)))
                                                                && (toDate == null || (toDate != null && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(toDate)))
                                                                && x.StoreId == storeId
                                                                && (String.IsNullOrEmpty(userName) || x.CreatedUser == userName)
                                                                && (docType == null || x.DocumentType == docType)
                                                                && !x.IsDeleted)
                                                          .AsEnumerable().Where(c => notes == null || notes.Count == 0 || notes.Any(m => m == c.NoteId))
                                                          .OrderByDescending(m => m.CreatedDate).ToList();

                var result = _mapper.Map<IEnumerable<Tb_CashBook>, IEnumerable<CashBookModals>>(model);
                result = result.Join(_unitOfWork.DocumentRepository.GetAllData()
                                        , m => m.DocumentType
                                        , n => n.DocType
                                        , (m, n) => new { m, n })
                               .Select(x => { x.m.DocumentTypeString = x.n.Description; return x; })
                               .Select(x => x.m).ToList();
                result.Join(_unitOfWork.AccountRepository.GetAllData(),
                            m => m.CreatedUser,
                            n => n.Username,
                            (m, n) => new { m, n })
                      .Select(x => { x.m.FullName = x.n.Firstname + " " + x.n.Lastname; return x; })
                      .Select(x => x.m).ToList();

                if (model != null && result != null && result.Any())
                {
                    result.ElementAt(0).TotalCreditAccount = model.Sum(s => s.CreditAccount) ?? 0;
                    result.ElementAt(0).TotalDebitAccount = model.Sum(s => s.DebitAccount) ?? 0;
                }
                for (int i = 0; i < 8; i++)
                    dt.Columns.Add(i + "", typeof(string));
                dt.Rows.Add("Loại Hình", "Mã HĐ", "Khách hàng", "Người Giao Dịch", "Ngày", "Diễn Giải", "Thu", "Chi");
                foreach (var item in result)
                {
                    dt.Rows.Add(item.DocumentTypeString, item.DocumentName, item.Customer, item.FullName, item.DocumentDate, item.Note, item.CreditAccount.ToPrice(), item.DebitAccount.ToPrice());
                }
                if (model != null && result != null && result.Any())
                {
                    dt.Rows.Add("", "", "", "", "", "Tổng cộng:", result.ElementAt(0).TotalCreditAccount.ToPrice(), result.ElementAt(0).TotalDebitAccount.ToPrice());
                    dt.Rows.Add("", "", "", "", "", "Tổng thu - chi:", (result.ElementAt(0).TotalDebitAccount - result.ElementAt(0).TotalCreditAccount).ToPrice());
                }
                stream = excelPackage.ExportToExcelCashBook(dt);
            }
            catch (Exception ex)
            {
                PawnLog.Error("CashBookServices --> ExportExcel ", ex);
                stream = null;
            }
            return stream;
        }

    }
}
