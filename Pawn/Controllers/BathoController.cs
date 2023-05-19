using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawn.Services;
using Pawn.ViewModel.Models;
using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.App_Start;
using Newtonsoft.Json;
using System.Dynamic;
using Pawn.Models;

namespace Pawn.Controllers
{
    public class BathoController : BaseController
    {
        private IBathoServices _bathoServices;
        private IHistoryServices _historyServices;
        private IFileServices _file;
        public BathoController(IBathoServices bathoServices, IHistoryServices historyServices, IFileServices file)
        {
            _bathoServices = bathoServices;
            _historyServices = historyServices;
            _file = file;
        }
        // GET: Batho
        public ActionResult Index()
        {
            ViewBag.ContractCode = _bathoServices.GetMaxBHContract();
            return View();
        }

        #region Code A T.A
        public ActionResult AddNewBH(BatHoModels model)
        {
            model.IsCloseContract = false;
            model.CreatedDate = DateTime.Now;
            model.CreatedUser = RDAuthorize.Username;
            model.StoreId = RDAuthorize.Store.Id;
            model.DocumentName = model.Code;
            model.DocumentType = (int)(DocumentTypeEnum.BatHo);
            model.DocumentDate = DateTime.Now;
            var result = model.Id < 1 ?_bathoServices.AddBHContract(model) : _bathoServices.UpdateBatHo(model);
            return Json(result);
        }

        //public ActionResult LoadDataBatho(int currentPage, int pageSize, string parameters)
        //{
        //    var objSearch = parameters.ToObject<ParametersModels>();
        //    var result = _bathoServices.LoadDataBatHo(objSearch.ContractCode, objSearch.customerName, objSearch.fromDate, objSearch.toDate,
        //        objSearch.DatediffCreateContract, objSearch.StatusContractId, pageSize, currentPage);
        //    return Json(new
        //    {
        //        totalRows = result.Select(m => m.TotalRows).FirstOrDefault(),
        //        data = result,
        //        addition = ""
        //    }, JsonRequestBehavior.AllowGet);
        //}

        private List<FilterItemModel> GetTimePawn()
        {
            List<FilterItemModel> lsResult = new List<FilterItemModel> { new FilterItemModel { Code = -1, Description = "Tất cả" } };
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
            foreach (StatusContractEnum item in Enum.GetValues(typeof(StatusContractEnum)))
            {
                int value = (int)Enum.Parse(typeof(StatusContractEnum), item.ToString());
                lsResult.Add(new FilterItemModel()
                {
                    Code = value,
                    Description = item.DescriptionAttr()
                });
            }
            return lsResult;
        }
        #endregion

        #region Code A Nam

        [HttpPost]
        public JsonResult GetAllData(int currentPage, int pageSize, string keyword, string parameters)
        {
            var parameter = JsonConvert.DeserializeObject<ParametersModels>(parameters);
            //// start service
            var objSearch = parameters.ToObject<ParametersModels>();
            var timePawns = GetTimePawn(); // get danh sách thời gian vay cho filter Object :Code, Description:
            var documentStatus = GetStatusContract(); //get danh sách tình trạng vay cho filter ::Code, Description
            var data = _bathoServices.LoadDataBatHo(objSearch.documentName, objSearch.customerName, objSearch.fromDate, objSearch.toDate,
                   objSearch.timePawn, objSearch.documentStatus, pageSize, currentPage, objSearch.columnsort, objSearch.sorttype, objSearch.StaffManagerId ?? -1);

            // cũng gọi là, biết troll nhau đấy chứ, gọi xong trả về "" thì dat ở đâu
            /// end service
            return Json(new
            {
                totalRows = data.FirstOrDefault()?.TotalRows ?? 0, //truyen data.totalRows,
                data = data,
                addition = new { timePawns, documentStatus }
            }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult LoadHistoryRemind(int contractId)
        {
            var remind = _historyServices.LoadDataHistory(contractId, (int)DocumentTypeEnum.RemindB, RDAuthorize.Store.Id);
            return Json(new { historyRemind = remind }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadHistoryAction(int contractId)
        {
            var action = _historyServices.LoadDataHistory(contractId, (int)DocumentTypeEnum.BatHo, RDAuthorize.Store.Id);
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
                TypeHistory = (int)DocumentTypeEnum.RemindB
            };
            var result = _historyServices.AddHistoryMessage(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TurnAroundBatHo(BatHoModels newModel, int originalId)
        {
            var result = _bathoServices.CloseContract(originalId, newModel.MoneyOrther);
            // tạo bát họ mới
            if (result.Type == MessageTypeEnum.Success)
            {
                var dateNow = DateTime.Now;
                newModel.IsCloseContract = false;
                newModel.CreatedDate = dateNow;
                newModel.CreatedUser = RDAuthorize.Username;
                newModel.UpdatedDate = dateNow;
                newModel.UpdatedUser = RDAuthorize.Username;
                newModel.StoreId = RDAuthorize.Store.Id;
                newModel.Code = "BH-" + dateNow.Year.ToString() + dateNow.Month.ToString() + dateNow.Day + dateNow.Hour.ToString() + dateNow.Minute.ToString() + dateNow.Second.ToString();
                newModel.DocumentName = newModel.Code;
                newModel.DocumentType = (int)(DocumentTypeEnum.BatHo);
                newModel.DocumentDate = newModel.FromDate;
                newModel.IsSystem = true;
                result = _bathoServices.AddBHContract(newModel);
            }
            if (result.Type == MessageTypeEnum.Success) result.Message = "Đảo họ thành công!";
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Code Tuan
        public ActionResult LoadDetailBatHo(int id)
        {
            var result = _bathoServices.LoadDetailBatHo(id);
            return Json(result);
        }

        public ActionResult UpdateBh(BatHoPayModels item)
        {
            var result = _bathoServices.BatHoPay(item);
            return Json(result);
        }

        public ActionResult GetMaxContractBHCode()
        {
            var result = _bathoServices.GetMaxBHContract();
            return Json(result);
        }

        public ActionResult UpdateBhPay(decimal money, int id)
        {
            var result = _bathoServices.UpdateBatHoPay(money, id);
            return Json(result);
        }

        public ActionResult CloseContract(int intBhId, decimal moneyNumber)
        {
            var result = _bathoServices.CloseContract(intBhId, moneyNumber);
            return Json(result);
        }

        public ActionResult LoadDetailBatHoModel(int id)
        {
            var result = _bathoServices.LoadDetailBatHoModel(id);
            return Json(result);
        }

        public ActionResult UpdateBadDebt(int idBatHo, bool isBadDebt)
        {
            var result = _bathoServices.UpdateBadDebt(idBatHo, isBadDebt);
            return Json(result);
        }
        public ActionResult DeleteBatHo(int idBatHo) => Json(_bathoServices.DeleteBatHo(idBatHo));
        public FileResult ExportExcel()
        {
            return File(_bathoServices.ExportExcel().ToArray(), "application/vnd.ms-excel",
                   $"BatHo_{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx");
        }

        public ActionResult ConvertStore(int bathoId, int storeId)
        {
            var rs = _bathoServices.ConvertStore(bathoId, storeId);
            return Json(rs);
        }
        public ActionResult UpdateMoneySI(int contractId, decimal? moneyIntroduce, decimal? moneyService)
        {
            var result = _bathoServices.UpdateMoneySI(contractId, moneyIntroduce, moneyService);
            return Json(result);
        }
        #endregion
    }
}