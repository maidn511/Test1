using Pawn.Authorize;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class CustomerController : BaseController
    {
        private ICustomerServices _customer;
        public CustomerController(ICustomerServices customer)
        {
            _customer = customer;
        }
        // GET: Customer
        public ActionResult Index()
        {
            if (!RDAuthorize.IsPermissionConfig("CustomerView")) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        public ActionResult AddCustomer(int? id)
        {
            if (id.HasValue) if (!RDAuthorize.IsPermissionConfig("CustomerEdit")) return RedirectToAction("Index");
            if (!id.HasValue) if (!RDAuthorize.IsPermissionConfig("CustomerAdd")) return RedirectToAction("Index");
            if (!id.HasValue) return View(new CustomerModels());
            var objCustomer = _customer.LoadDetailCustomer(id.Value);
            return View(objCustomer);
        }

        [HttpPost]
        public ActionResult AddCustomer(CustomerModels objCustomer)
        {
            if (objCustomer.Id > 0) if (!RDAuthorize.IsPermissionConfig("Customerdit")) return RedirectToAction("Index");
            if (objCustomer.Id <= 0) if (!RDAuthorize.IsPermissionConfig("CustomerAdd")) return RedirectToAction("Index");
            objCustomer.CreatedDate = DateTime.Now;
            objCustomer.CreatedUser = RDAuthorize.Username;
            var isError = _customer.AddCustomer(objCustomer);
            return Json(isError);
        }

        public ActionResult _PartialCustomer(string strKeyword = "", int intStatusId = -1, int intPageIndex = 1)
        {
            var data = _customer.LoadDataCustomer(strKeyword, intStatusId, pageSize, intPageIndex);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = pageSize * (intPageIndex - 1) + 1;
            return PartialView(data);
        }

        public ActionResult _PartialCustomerModal()
        {
            var data = _customer.LoadDataCustomer("", 1, -1, 1);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = 1;
            return PartialView(data);
        }

        public ActionResult _PatialAssignCustomer(int storeId = -1)
        {
            var data = _customer.LoadDataCustomer(null, -1, -1, -1, storeId);
            return PartialView(data);
        }

        public ActionResult DeleteCustomer(int intCustomerId)
        {
            if (!RDAuthorize.IsPermissionConfig("CustomerDelete")) return Json(true);
            var data = _customer.DeleteCustomer(intCustomerId, RDAuthorize.Username);
            return Json(data);
        }

        public ActionResult AssignCustomer()
        {
            return View();
        }
    }
}