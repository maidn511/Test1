using Newtonsoft.Json;
using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class IncomeAndExpenseController : BaseController
    {

        private IIncomeAndExpenseServices _incomeAndExpenseService;
        private IStatusServices _statusService;
        public IncomeAndExpenseController(IIncomeAndExpenseServices incomeAndExpenseService, IStatusServices statusService)
        {
            _incomeAndExpenseService = incomeAndExpenseService;
            _statusService = statusService;
        }
        // GET: IncomeAndExpense
        public ActionResult Index()
        {
            if (!RDAuthorize.IsPermissionConfig("IncomeAndExpenseView")) return RedirectToAction("Index", "Home");
            return View();
        }

        public ActionResult Index2()
        {
            if (!RDAuthorize.IsPermissionConfig("IncomeAndExpenseView")) return RedirectToAction("Index", "Home");
            return View();
        }

        //GET: List
        [HttpPost]
        public JsonResult GetAllData(int currentPage, int pageSize, string keyword, string parameters)
        {
            // B1: parse parameter
            // B2: processing
            var parameter = JsonConvert.DeserializeObject<ParametersModels>(parameters);
            var data = _incomeAndExpenseService.GetAllData(RDAuthorize.Store.Id,parameter.voucherType??0, parameter.Method, parameter.fromDate ,
                parameter.toDate ,currentPage,pageSize).ToList();
            var statusList = _statusService.LoadDataStatus((parameter.voucherType == 1?4:1), null, 1, -1, -1);
            var result = data.Join(statusList,
                m => m.Method,
                n => n.Id,
                (m, n) =>new { m, n }).Select(x => { x.m.MethodString = x.n.StatusName; return x; }).Select(x=>x.m).ToList();
            return Json(new { totalRows = data.Select(m => m.TotalRows).FirstOrDefault(),
                data = result, addition = "" }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult AddIncomeAndExpense(IncomeAndExpenseModels model, int type)
        {
            var date = DateTime.Now;
            string barcode = date.Year.ToString() + date.Month.ToString() + date.Day + date.Hour.ToString() + date.Minute.ToString() + date.Second.ToString();
            string code = (type == 1) ? "PT_"+ barcode : "PC_"+ barcode;
            if (model.Id > 0) if (!RDAuthorize.IsPermissionConfig("IncomeAndExpensedit")) return RedirectToAction("Index");
            if (model.Id <= 0) if (!RDAuthorize.IsPermissionConfig("IncomeAndExpenseAdd")) return RedirectToAction("Index");
            model.StoreId = RDAuthorize.Store.Id;
            model.CreatedUser = RDAuthorize.Username;
            model.CreatedDate = date;
            model.UpdatedUser = RDAuthorize.Username;
            model.UpdatedDate = date;
            model.DocumentName = code;
            model.DocumentType = (int)DocumentTypeEnum.ThuChiHoatDong;
            model.DocumentDate = date;
            model.IsActive = true;
            model.VoucherCode = code;
            model.VoucherType = type;
            var result = _incomeAndExpenseService.AddIncomeAndExpense(model,type);
            return Json(result);
        }

        public ActionResult _PartialIncomeAndExpense(string strKeyword = "", int intIsActive = -1, int intPageIndex = 1)
        {
            var data = _incomeAndExpenseService.LoadDataIncomeAndExpense(strKeyword, intIsActive, pageSize, intPageIndex);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = pageSize * (intPageIndex - 1) + 1;
            return PartialView(data);
        }

        public ActionResult DeleteIncomeAndExpense(int id)
        {
            if (!RDAuthorize.IsPermissionConfig("IncomeAndExpenseDelete")) return Json(true);
            var data = _incomeAndExpenseService.DeleteIncomeAndExpense(id, RDAuthorize.Username);
            return Json(data);
        }
    }
}