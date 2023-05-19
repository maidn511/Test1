using Newtonsoft.Json;
using Pawn.Libraries;
using Pawn.Models;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawn.Authorize;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class PawnController : BaseController
    {
        private IPawnServices _pawnServices;
        private IHistoryServices _historyServices;
        private ICustomerServices _customerServices;
        private ICashBookServices _cashBookServices;
        private IFileServices _file;
        private readonly ICapitalLoanServices _capitalLoan;
        private readonly IExtentionContractServices _extentionContract;

        public PawnController(IPawnServices pawnServices, ICustomerServices customerServices, IHistoryServices historyServices, IFileServices file,
                              ICapitalLoanServices capitalLoan, IExtentionContractServices extentionContract, ICashBookServices cashBookServices)
        {
            _pawnServices = pawnServices;
            _historyServices = historyServices;
            _customerServices = customerServices;
            _file = file;
            _capitalLoan = capitalLoan;
            _extentionContract = extentionContract;
            _cashBookServices = cashBookServices;
        }
        // GET: Pawn
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetAllData(int currentPage, int pageSize, string keyword, string parameters)
        {
            //// start service
            var parameter = parameters.ToObject<ParametersModels>();
            var timePawns = GetTimePawn(); // get danh sách thời gian vay cho filter Object :Code, Description:
            var documentStatus = GetStatusContract(); //get danh sách tình trạng vay cho filter ::Code, Description
            var data = _pawnServices.GetAllData(RDAuthorize.Store.Id, parameter.fromDate,
                parameter.toDate, currentPage, pageSize, parameter.customerId,
                parameter.documentName, parameter.documentStatus, parameter.columnsort, parameter.sorttype, parameter.StaffManagerId ?? -1);
            var customers = _customerServices.GetCustomerByStoreId(RDAuthorize.Store.Id);
            return Json(new
            {
                totalRows = data?.FirstOrDefault()?.TotalRows ?? 0,
                data = data,
                addition = new { timePawns = timePawns, documentStatus = documentStatus, customers = customers }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetDetailById(int contractId)
        {
            var data = _pawnServices.GetDetailById(RDAuthorize.Store.Id, contractId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetPawnPaysById(int id)
        {
            var data = _pawnServices.GetPawnPaysById(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DongLai(PawnContractModels header, PawnPayModels item)
        {
            var data = _pawnServices.DongLai(header, item);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DongLaiTuyBien(PawnContractModels header, PawnPayModels item)
        {
            var data = _pawnServices.DongLaiTuyBien(header, item);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        private List<FilterItemModel> GetTimePawn()
        {
            List<FilterItemModel> lsResult = new List<FilterItemModel>();
            foreach (TimePawnDate item in Enum.GetValues(typeof(TimePawnDate)))
            {
                int value = (int)Enum.Parse(typeof(TimePawnDate), item.ToString());
                lsResult.Add(new FilterItemModel()
                {
                    Code = value,
                    Description = value.ToString() + " Ngày",
                });
            }
            return lsResult;
        }

        private List<FilterItemModel> GetStatusContract()
        {
            List<FilterItemModel> lsResult = new List<FilterItemModel>();
            var enumstc = Enum.GetValues(typeof(StatusContractPawnEnum)).Cast<StatusContractPawnEnum>();
            foreach (StatusContractPawnEnum item in enumstc)
            {
                int value = (int)Enum.Parse(typeof(StatusContractPawnEnum), item.ToString());
                lsResult.Add(new FilterItemModel()
                {
                    Code = value,
                    Description = item.DescriptionAttr()
                });
            }
            return lsResult;
        }


        [HttpPost]
        public JsonResult DongHopDong(int contractId, double totalMoney)
        {
            var data = _pawnServices.DongHopDong(contractId, totalMoney);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GiaHanThem(PawnContractModels model, ExtentionContractModels extentionContract)
        {
            //// Update tổng số ngày vay
            //// Generate lại kỳ
            var rs = _extentionContract.AddExtentionContract(extentionContract);
            var res = _pawnServices.GiaHanThem(model, extentionContract.AddTime);
            return Json(res);
        }

        #region T.A
        public ActionResult Addnewpawn(PawnContractModels model)
        {
            model.IsCloseContract = false;
            model.CreatedDate = DateTime.Now;
            model.CreatedUser = RDAuthorize.Username;
            model.UpdatedDate = DateTime.Now;
            model.UpdatedUser = RDAuthorize.Username;
            model.StoreId = RDAuthorize.Store.Id;
            model.DocumentName = model.Code;
            model.PawnDate = model.PawnDatePost;
            model.DocumentType = (int)(DocumentTypeEnum.VayLai);
            model.DocumentDate = model.DocumentDatePost;
            var result = _pawnServices.AddPawnContract(model);
            return Json(result);
        }

        public ActionResult LoadInterestRateType()
        {
            List<FilterItemModel> lsResult = new List<FilterItemModel>();
            foreach (InterestRateTypeEnum item in Enum.GetValues(typeof(InterestRateTypeEnum)))
            {
                int value = (int)Enum.Parse(typeof(InterestRateTypeEnum), item.ToString());
                lsResult.Add(new FilterItemModel()
                {
                    Code = value,
                    Description = item.DescriptionAttr()
                });
            }
            return Json(lsResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadHistoryRemind(int contractId)
        {
            var remind = _historyServices.LoadDataHistory(contractId, (int)DocumentTypeEnum.RemindV, RDAuthorize.Store.Id);
            return Json(new { historyRemind = remind }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadHistoryAction(int contractId)
        {
            var action = _historyServices.LoadDataHistory(contractId, (int)DocumentTypeEnum.VayLai, RDAuthorize.Store.Id);
            return Json(new { historyAction = action }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddHistoryRemind(string content, int contractId)
        {
            var model = new HistoryModels
            {
                CreatedBy = RDAuthorize.Username,
                StoreId = RDAuthorize.Store.Id,
                CreatedDate = DateTime.Now,
                Content = content,
                ContractID = contractId,
                TypeHistory = (int)DocumentTypeEnum.RemindV
            };
            var result = _historyServices.AddHistoryMessage(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Code Tuấn em
        public ActionResult LoadDetailPawnContract(int idContract) => Json(_pawnServices.LoadDetailPawnContract(idContract));

        [HttpPost]
        public ActionResult AddWithDrawCapital(PawnContractModels header, CapitalLoanModels objCapitalLoan)
        {
            objCapitalLoan.DocumentType = (int)DocumentTypeEnum.VayLai;
            var result = _capitalLoan.AddAddWithDrawCapital(objCapitalLoan);
            if (result.Type == MessageTypeEnum.Success)
            {
                _historyServices.AddHistoryMessage(new HistoryModels
                {
                    ContractID = header.Id,
                    Content = objCapitalLoan.IsLoan ? "Vay thêm gốc" : "Trả gốc",
                    StoreId = RDAuthorize.Store.Id,
                    TypeHistory = (int)DocumentTypeEnum.VayLai,
                    DebitMoney = objCapitalLoan.IsLoan ? objCapitalLoan.MoneyNumber : 0,
                    HavingMoney = objCapitalLoan.IsLoan ? 0 : objCapitalLoan.MoneyNumber,
                    CreatedBy = RDAuthorize.Username,
                    CreatedDate = DateTime.Now
                });

                _cashBookServices.AddCashBook(new CashBookModals
                {
                    CreatedDate = DateTime.Now,
                    Customer = header.CustomerName,
                    CreatedUser = RDAuthorize.Username,
                    CreditAccount = objCapitalLoan.IsLoan ? 0 : objCapitalLoan.MoneyNumber,
                    DebitAccount = objCapitalLoan.IsLoan ? objCapitalLoan.MoneyNumber : 0,
                    Note = objCapitalLoan.IsLoan ? "Vay thêm gốc" : "Trả gốc",
                    DocumentDate = DateTime.Now,
                    DocumentType = (int)DocumentTypeEnum.VayLai,
                    VoucherType = objCapitalLoan.IsLoan ? (int)VocherTypeEnum.PhieuChi : (int)VocherTypeEnum.PhieuThu,
                    StoreId = RDAuthorize.Store.Id,
                    IsActive = true,
                    IsDeleted = false,
                    ContractId = header.Id
                });
                var kq = _pawnServices.TraBotGoc(header);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadDataCapitalLoan(int id) => Json(_capitalLoan.LoadDataCapitalLoan(id, DocumentTypeEnum.VayLai));
        public ActionResult DeleteCapitalLoan(PawnContractModels header, int id)
        {
            var result = _capitalLoan.DeleteCapitalLoan(id, DocumentTypeEnum.VayLai);
            if (result.Type == MessageTypeEnum.Success)
            {
                var kq = _pawnServices.TraBotGoc(header);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetMaxContractVLCode()
        {
            var result = _pawnServices.GetMaxVLContract();
            return Json(result);
        }

        public ActionResult DeletePawn(int idPawn) => Json(_pawnServices.DeletePawn(idPawn));

        public FileResult ExportExcel()
        {
            return File(_pawnServices.ExportExcel().ToArray(), "application/vnd.ms-excel",
                   $"VayLai_{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx");
        }

        public ActionResult ConvertStore(int vlId, int storeId)
        {
            var rs = _pawnServices.ConvertStore(vlId, storeId);
            return Json(rs);
        }

        public ActionResult UpdateBadDebt(int idVayLai, bool isBadDebt)
        {
            var result = _pawnServices.UpdateBadDebt(idVayLai, isBadDebt);
            return Json(result);
        }

        public ActionResult UpdateMoneySI(int contractId, decimal? moneyIntroduce, decimal? moneyService)
        {
            var result = _pawnServices.UpdateMoneySI(contractId, moneyIntroduce, moneyService);
            return Json(result);
        }
        #endregion
    }
}