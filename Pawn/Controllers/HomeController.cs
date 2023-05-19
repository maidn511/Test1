using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Pawn.Controllers
{
    public class HomeController : BaseController
    {
        private IMenuServices _menu;
        private readonly IStoreServices _store;
        private readonly IBathoServices _batho;
        private readonly IPawnServices _pawn;
        public HomeController(IMenuServices menu, IStoreServices store, IBathoServices batho, IPawnServices pawn)
        {
            _menu = menu;
            _store = store;
            _batho = batho;
            _pawn = pawn;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AccessDenied()
        {
            Response.StatusCode = 401;
            return View();
        }

        public ActionResult _PartialStore()
        {
            return PartialView();
        }

        public ActionResult _PartialCustomerPaidTomorow(InterestPaid addDay)
        {
            ViewBag.DataCustomerBatHo = _batho.LoadDataCustomerPaidTomorrow(addDay);
            ViewBag.DataCustomerVayLai = _pawn.LoadDataCustomerPaidTomorrow(addDay);
            ViewBag.Type = addDay;
            return PartialView();
        }

        public FileResult ExportExcel(bool isVl, InterestPaid addDay)
        {
            var excel = new ExcelPackages($"Ds Khách hàng đóng lãi {(addDay == InterestPaid.Tomorrow ? "ngày mai" : "hôm nay")}");
            var dt = new DataTable();
            dt.Columns.Add("1", typeof(string));
            dt.Columns.Add("2", typeof(string));
            dt.Columns.Add("3", typeof(string));

            dt.Rows.Add("Tên khách hàng", "Số tiền trên ngày", $"Tổng tiền phải đóng {(addDay == InterestPaid.Tomorrow ? "đến ngày mai" : "hôm nay")}");
            if (isVl)
            {
                var data = _pawn.LoadDataCustomerPaidTomorrow(addDay);
                if (data.Any() == false)
                {
                    return File(excel.ExportExcelNoData().ExportExcel(), "application/vnd.ms-excel",
                  $"KH_DongLai_{(addDay == InterestPaid.Tomorrow ? "NgayMai" : "HomNay")}_{(isVl ? "VayLai" : "BatHo")}_{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx");
                }
                else
                    foreach (var item in data)
                        dt.Rows.Add(item.CustomerName, item.TienLaiMotNgay.ToPrice(), item.TotalMoney.ToPrice());
            }
            else
            {
                var data = _batho.LoadDataCustomerPaidTomorrow(addDay);
                if (data.Any() == false)
                {
                    return File(excel.ExportExcelNoData().ExportExcel(), "application/vnd.ms-excel",
                  $"KH_DongLai_{(addDay == InterestPaid.Tomorrow ? "NgayMai" : "HomNay")}_{(isVl ? "VayLai" : "BatHo")}_{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx");
                }
                else
                    foreach (var item in data)
                    dt.Rows.Add(item.CustomerName, item.MoneyPerDay.ToPrice(), item.TotalMoney.ToPrice());
            }

            MemoryStream stream = excel.ExportToExcel(dt);

            return File(stream, "application/vnd.ms-excel",
                  $"KH_DongLai_{(addDay == InterestPaid.Tomorrow ? "NgayMai" : "HomNay")}_{(isVl ? "VayLai" : "BatHo")}_{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx");
        }

        public int Count(bool isBH, InterestPaid addDay)
        {
            if (isBH) return _batho.LoadDataCustomerPaidTomorrow(addDay).Count;
            else return _pawn.LoadDataCustomerPaidTomorrow(addDay).Count;
        }

        public PartialViewResult Menu()
        {
            bool bolTemp = false;
            int intTemp = 0;
            List<MenuModels> lstMenu = new List<MenuModels>();
            bool IsRoot = Session[Constants.isRoot] != null && bool.TryParse(Session[Constants.isRoot].ToString(), out bolTemp) ? bool.Parse(Session[Constants.isRoot].ToString()) : false;
            int UserID = Session[Constants.userid] != null && int.TryParse(Session[Constants.userid].ToString(), out intTemp) ? int.Parse(Session[Constants.userid].ToString()) : -1;
            if (IsRoot)
            {
                lstMenu = _menu.LoadDataMenu(-1, "", -1, 1, -1, -1).OrderBy(c => c.OrderIndex).ToList();
            }
            else
            {
                lstMenu = _menu.LoadDataMenuByUser(UserID).OrderBy(c => c.OrderIndex).ToList();
            }
            return PartialView(lstMenu);
        }
    }
}