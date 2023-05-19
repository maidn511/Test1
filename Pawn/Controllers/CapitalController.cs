using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class CapitalController : BaseController
    {
        private readonly ICapitalServices _capital;
        private readonly ICapitalLoanServices _capitalLoan;
        public CapitalController(ICapitalServices capital, ICapitalLoanServices capitalLoan)
        {
            _capital = capital;
            _capitalLoan = capitalLoan;
        }
        // GET: Capital
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadDataCapital(int currentPage, int pageSize, string keyword, string parameters)
        {
            var objSearch = parameters.ToObject<ParametersModels>();
            var result = _capital.LoadDataCapital(objSearch.Keyword, objSearch.toDate, objSearch.fromDate, objSearch.StatusContractId, pageSize, currentPage);
            return Json(new
            {
                totalRows = result.Select(m => m.TotalRows).FirstOrDefault(),
                data = result,
                addition = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddCapital(CapitalModels model)
        {
            if (model.Id <= 0) if (!RDAuthorize.IsPermissionConfig("AccountAdd")) return RedirectToAction("Index");
            if (model.Id > 0) if (!RDAuthorize.IsPermissionConfig("AccountEdit")) return RedirectToAction("Index");
            model.CreatedUser = RDAuthorize.Username;
            model.StoreId = RDAuthorize.Store.Id;
            model.DocumentDate = Convert.ToDateTime(model.DocumentDatePost);
            var rs = _capital.AddCapital(model);
            return Json(rs);
        }

        [HttpPost]
        public ActionResult LoadDetailCapital(int id)
        {
            var data = _capital.LoadDetailCapital(id);
            return Json(new { data });
        }

        public ActionResult LoadCapitalDetail(int id)
        {
            var result = _capital.LoadCapitalDetail(id);
            return Json(result);
        }

        [HttpPost]
        public ActionResult AddWithDrawCapital(CapitalLoanModels objCapitalLoan)
        {
            objCapitalLoan.DocumentType = (int)DocumentTypeEnum.NguonVon;
            var result = _capitalLoan.AddAddWithDrawCapital(objCapitalLoan);
            return Json(result);
        }

        public ActionResult LoadDataCapitalLoan(int id) => Json(_capitalLoan.LoadDataCapitalLoan(id, DocumentTypeEnum.NguonVon));
        public ActionResult DeleteCapitalLoan(int id) => Json(_capitalLoan.DeleteCapitalLoan(id, DocumentTypeEnum.NguonVon));

        public ActionResult AddCapitalPayDays(CapitalPayDayModels objCapitalPayDays)
        {
            var result = objCapitalPayDays.Id < 1 ? _capital.AddCapitalPayDay(objCapitalPayDays) : _capital.DeleteCapitalPayDay(objCapitalPayDays);
            return Json(result);
        }

        public ActionResult CalculatorMoneyPerDayPay(CapitalDetailModels capitalDetail, List<CapitalLoanModels> lstCapitalLoans, DateTime dtFromDate, DateTime dtToDate)
        {
            var rs = _capital.GetMoneyPerDayPayDay(capitalDetail, lstCapitalLoans, dtFromDate, dtToDate);
            return Json(rs);
        }

        public ActionResult CloseContract(int id, decimal moneyNumber) => Json(_capital.CloseContract(id, moneyNumber));
    }
}