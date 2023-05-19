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
    public class StoreController : BaseController
    {

        private IStoreServices _store;
        public StoreController(IStoreServices store)
        {
            _store = store;
        }
        // GET: Store
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddStore(int? id)
        {
            if (!id.HasValue) return View(new PawnStoreModels());
            var objStore = _store.LoadDetailStore(id.Value);
            if (objStore == null) return RedirectToAction("_403", "Error");
            return View(objStore);
        }

        [HttpPost]
        public ActionResult AddStore(PawnStoreModels objStore)
        {
            objStore.CreatedUser = RDAuthorize.Username;
            var isError = _store.AddStore(objStore);
            return Json(isError);
        }

        public ActionResult _PartialStore(string strKeyword = "", int intIsActive = -1, int intPageIndex = 1)
        {
            var data = _store.LoadDataStore(strKeyword, intIsActive, pageSize, intPageIndex);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = pageSize * (intPageIndex - 1) + 1;
            return PartialView(data);
        }

        public ActionResult _PartialStoreModal()
        {
            var data = _store.LoadDataStore("", 1, -1, 1);
            ViewBag.TotalRows = data.Count > 0 ? data[0].TotalRows : 0;
            ViewBag.Index = 1;
            return PartialView(data);
        }

        public ActionResult DeleteStore(int intStoreId)
        {
            var data = _store.DeleteStore(intStoreId, RDAuthorize.Username);
            return Json(data);
        }
        public ActionResult GetStoreAfterLogin()
        {
            var Store = RDAuthorize.Store;
            var StoreList = RDAuthorize.StoreList;
            return Json(new { store = Store, storeList = StoreList}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetStore(int storeId)
        {
            var s = _store.LoadDetailStore(storeId);
            if(s == null) return RedirectToAction("_403", "Error");
            Session[Constants.store] = s;
            return RedirectToAction("Index");
        }
    }
}