using Pawn.Authorize;
using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class CommonController : BaseController
    {
        private readonly IAccountServices _account;
        private readonly IAccountTypeServices _accountType;
        private readonly IStoreServices _store;
        private readonly IStatusServices _status;
        private readonly IHistoryServices _history;
        private readonly IExtentionContractServices _extentionContract;
        private readonly IDebtServices _debt;
        private readonly IFileServices _file;
        private readonly ITimerServices _timer;
        private readonly ICustomerServices _customer;

        public CommonController(IAccountServices account, IAccountTypeServices accountType,
                                IStoreServices store, IStatusServices status, IHistoryServices history, 
                                IExtentionContractServices extentionContract, IDebtServices debt, IFileServices file,
                                ITimerServices timer, ICustomerServices customer)
        {
            _account = account;
            _accountType = accountType;
            _store = store;
            _status = status;
            _history = history;
            _extentionContract = extentionContract;
            _debt = debt;
            _file = file;
            _timer = timer;
            _customer = customer;
        }
        public ActionResult LoadSelectMenu(SelectOptionModels objSelectOptions)
        {
            var lstAccount = _account.GetListAccount(null, 1, -1, -1);
            if (lstAccount.Any())
                objSelectOptions.ListItems = lstAccount.Select(c => new SelectListItemModels { Value = c.Id + "", Text = c.AccountType + "" }).ToList();

            return PartialView("SelectCustom", objSelectOptions);
        }
        
        public ActionResult LoadSelectAccountType(SelectOptionModels objSelectOptions)
        {
            var lstAccountType = _accountType.LoadDataAccountType(null, 1, -1, -1);
            var lstData = new List<RoleModels>();
            foreach (var item in lstAccountType)
            {
                if (RDAuthorize.IsRoot && item.Id != (int)RoleEnum.Root)
                    lstData.Add(item);
                else if(RDAuthorize.IsAdmin && item.Id != (int)RoleEnum.Root && item.Id != (int)RoleEnum.Admin)
                    lstData.Add(item);
                else if(RDAuthorize.IsUserStore && item.Id != (int)RoleEnum.Root && item.Id != (int)RoleEnum.Admin)
                    lstData.Add(item);
                else if(RDAuthorize.IsUserStore && item.Id != (int)RoleEnum.Root && item.Id != (int)RoleEnum.Admin && item.Id != (int)RoleEnum.Store)
                    lstData.Add(item);
            }
            if (lstData.Any())
                objSelectOptions.ListItems = lstData.Select(c => new SelectListItemModels { Value = c.Id + "", Text = c.RoleName + "" }).ToList();

            return PartialView("SelectCustom", objSelectOptions);
        }

        public ActionResult LoadSelectStore(SelectOptionModels objSelectOptions)
        {
            var lstStore = _store.LoadDataStore(null, 1, -1, -1);
            if (lstStore.Any())
                objSelectOptions.ListItems = lstStore.Select(c => new SelectListItemModels { Value = c.Id + "", Text = c.Name + "" }).ToList();

            return PartialView("SelectCustom", objSelectOptions);
        }

        public ActionResult LoadSelectStoreWithoutCurrent()
        {
            var lstStore = _store.LoadDataStore(null, 1, -1, -1);
            //var currentStore = lstStore.FirstOrDefault(s => s.Id == RDAuthorize.Store.Id);
            //if (currentStore != null)
            //    lstStore.Remove(currentStore);
            return Json(lstStore);
        }

        public ActionResult LoadSelectStatus(SelectOptionModels objSelectOptions, int intType = -1)
        {
            var lstBallotType = _status.LoadDataStatus(intType, null, 1, -1, -1);
            if (lstBallotType.Any())
                objSelectOptions.ListItems = lstBallotType.Select(c => new SelectListItemModels { Value = c.Id + "", Text = c.StatusName + "" }).ToList();

            return PartialView("SelectCustom", objSelectOptions);
        }


        public ActionResult LoadSelectStatusAngular(int intType = -1)
        {
            var lstStatus = _status.LoadDataStatus(intType, null, 1, -1, -1);
            var result = lstStatus.Select(c => new SelectOptionModel { Id = c.Id, Text = c.StatusName + "", OrderIndex = c.OrderIndex }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadDataCustomer()
        {
            var lstCustomer = _customer.LoadDataCustomer(null, -1, -1, -1);
            var result = lstCustomer.Select(c => new SelectOptionModel { Id = c.Id, Text = c.Fullname + "" }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadDataHistory(int contractId, int historyType)
            => Json(_history.LoadDataHistory(contractId, historyType, RDAuthorize.Store.Id), JsonRequestBehavior.AllowGet);

        public ActionResult LoadSelectStatus(SelectOptionModels objSelectOptions)
        {
            objSelectOptions.ListItems = new List<SelectListItemModels>
            {
               new SelectListItemModels {Text = "Thân thiện", Value = "0"},
               new SelectListItemModels {Text = "Nợ xấu", Value = "1"},
               new SelectListItemModels {Text = "Nguy hiểm", Value = "2"},
               new SelectListItemModels {Text = "Tội phạm", Value = "3"},
               new SelectListItemModels {Text = "NGÀNH", Value = "4"},
            };

            return PartialView("SelectCustom", objSelectOptions);
        }

        public ActionResult AddBreadCrumb(BreadCrumbModels breadCrumb)
        {
            return PartialView("BreadCrumb", breadCrumb);
        }

        public ActionResult AddExtentionContract(ExtentionContractModels extentionContract)
        {
            var rs = _extentionContract.AddExtentionContract(extentionContract);
            return Json(rs);
        }

        public ActionResult AddDebt(DebtModels debtModels) => Json(_debt.AddDebt(debtModels));


        public ActionResult LoadChungtu(int contractId, DocumentTypeEnum documentType)
        {
            var result = _file.LoadFile(documentType, contractId);
            return Json(result);
        }

        public ActionResult AddFile(FileManagementModels obj)
        {
            var rs = _file.AddFile(obj);
            return Json(rs);
        }

        public ActionResult DeleteFile(int idFile)
        {
            var rs = _file.DeleteFile(idFile);
            return Json(rs);
        }

        public ActionResult LoadTimer(int contractId, int docType) => Json(_timer.LoadDataTimer(contractId, docType));
        public ActionResult AddTimer(TimerModels timer) => Json(_timer.AddTimer(timer));

        public ActionResult LoadSelectStaffByStore()
        {
            var lstStatus = _account.GetListAccountByStoreId();
            return Json(lstStatus, JsonRequestBehavior.AllowGet);
        }
    }
}