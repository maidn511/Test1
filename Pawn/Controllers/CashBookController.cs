using Newtonsoft.Json;
using Pawn.Authorize;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawn.Libraries;

namespace Pawn.Controllers
{
    public class CashBookController : BaseController
    {
        private ICashBookServices _cashBookService;
        private IStatusServices _statusService;
        private IAccountServices _accountService;
        private IDocumentServices _documentService;
        public CashBookController(ICashBookServices cashBookService,
            IStatusServices statusService,
            IAccountServices accountService,
            IDocumentServices documentService)
        {
            _cashBookService = cashBookService;
            _statusService = statusService;
            _accountService = accountService;
            _documentService = documentService;
        }
        // GET: IncomeAndExpense
        public ActionResult Index()
        {
            //if (!RDAuthorize.IsPermissionConfig("IncomeAndExpenseView")) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public JsonResult GetAllData(int currentPage, int pageSize, string keyword, string parameters)
        {
            // B1: parse parameter
            // B2: processing
            var parameter = JsonConvert.DeserializeObject<ParametersModels>(parameters);
            var data = _cashBookService.GetAllData(RDAuthorize.Store.Id, parameter.fromDate,
                parameter.toDate, currentPage, pageSize, parameter.userName, parameter.documentType, parameter.Notes).ToList();
            var summary = _cashBookService.GetSummaryData(RDAuthorize.Store.Id, parameter.fromDate,
                parameter.toDate);
            var document = _documentService.GetAllData();
            var employee = _accountService.GetListAccountByLevel(RDAuthorize.UserId);

            return Json(new
            {
                totalRows = data.Select(m => m.TotalRows).FirstOrDefault(),
                data = data,
                addition = new { summary = summary, employee = employee, document = document }
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNotesData(int? docType)
        {
            var notes = NoteData();
            var data = docType != null ? notes.Where(m => m.DocType == docType) : notes;
            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 36000, VaryByParam = "none")]
        public IEnumerable<NoteModel> NoteData()
        {
            //var drama = new List<NoteModel>
            //{
            //    new NoteModel
            //    {
            //        Id = 1,
            //        Description = "Tạo mới hợp đồng bát họ",
            //        DocType = (int) DocumentTypeEnum.BatHo
            //    },
            //    new NoteModel
            //    {
            //        Id = 2,
            //        Description = "Giải ngân hợp đồng Bát họ",
            //        DocType = (int) DocumentTypeEnum.BatHo
            //    },
            //    new NoteModel
            //    {
            //        Id = 3,
            //        Description = "Tiền giới thiệu HD Bát họ",
            //        DocType = (int) DocumentTypeEnum.BatHo
            //    },
            //    new NoteModel
            //    {
            //        Id = 4,
            //        Description = "Tiền dịch vụ HD Bát họ",
            //        DocType = (int) DocumentTypeEnum.BatHo
            //    },
            //    new NoteModel
            //    {
            //        Id = 5,
            //        Description = "Đóng tiền họ",
            //        DocType = (int) DocumentTypeEnum.BatHo
            //    },
            //    new NoteModel
            //    {
            //        Id = 6,
            //        Description = "Hủy đóng tiền họ",
            //        DocType = (int) DocumentTypeEnum.BatHo
            //    },
            //    new NoteModel
            //    {
            //        Id = 7,
            //        Description = "Giải ngân hợp đồng Vay lãi",
            //        DocType = (int) DocumentTypeEnum.VayLai
            //    },
            //    new NoteModel
            //    {
            //        Id = 8,
            //        Description = "Tiền giới thiệu HD",
            //        DocType = (int) DocumentTypeEnum.VayLai
            //    },
            //    new NoteModel
            //    {
            //        Id = 9,
            //        Description = "Tiền dịch vụ HD Vay lãi",
            //        DocType = (int) DocumentTypeEnum.VayLai
            //    },
            //    new NoteModel
            //    {
            //        Id = 10,
            //        Description = "Đóng lãi",
            //        DocType = (int) DocumentTypeEnum.VayLai
            //    },
            //    new NoteModel
            //    {
            //        Id = 11,
            //        Description = "Hủy đóng lãi",
            //        DocType = (int) DocumentTypeEnum.VayLai
            //    },
            //    new NoteModel
            //    {
            //        Id = 13,
            //        Description = "Đóng hợp đồng",
            //        DocType = (int) DocumentTypeEnum.VayLai
            //    },

            //};
            List<NoteModel> drama = new List<NoteModel>();
            var enumstc = Enum.GetValues(typeof(NotesEnum)).Cast<NotesEnum>();
            foreach (NotesEnum item in enumstc)
            {
                int value = (int)Enum.Parse(typeof(NotesEnum), item.ToString());
                drama.Add(new NoteModel()
                {
                    Id = value,
                    DocType = item.DocumentTypeAttr(),
                    Description = item.DescriptionAttr()
                });
            }
            return drama;
        }

        public FileResult ExportExcel(string parameters)
        {
            var parameter = JsonConvert.DeserializeObject<ParametersModels>(parameters);
            return File(_cashBookService.ExportExcel(RDAuthorize.Store.Id, parameter.fromDate, parameter.toDate, parameter.userName, parameter.documentType, parameter.Notes).ToArray(), "application/vnd.ms-excel",
                   $"SoQuy_{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx");
        }
    }
}