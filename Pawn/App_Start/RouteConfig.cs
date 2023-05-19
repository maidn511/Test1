using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pawn
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            #region Account

            routes.MapRoute("AccountLogin", "login", new { controller = "Login", action = "Login" });
            routes.MapRoute("AccountLogout", "logout", new { controller = "Login", action = "Logout" });

            routes.MapRoute(
               "quan-ly-tai-khoan",
               "quan-ly-tai-khoan",
               new { controller = "Account", action = "Index" }
           );
            routes.MapRoute(
               "quan-ly-tai-khoan/them-tai-khoan",
               "quan-ly-tai-khoan/them-tai-khoan",
               new { controller = "Account", action = "AddAccount" }
           );
            routes.MapRoute(
               "quan-ly-tai-khoan/chinh-sua-tai-khoan",
               "quan-ly-tai-khoan/chinh-sua-tai-khoan/{username}",
               new { controller = "Account", action = "AddAccount", username = UrlParameter.Optional }
           );

            #endregion

            #region Role

            routes.MapRoute(
               "quan-ly-quyen",
               "quan-ly-quyen",
               new { controller = "Role", action = "Index" }
           );
            routes.MapRoute(
               "quan-ly-quyen/them-quyen",
               "quan-ly-quyen/them-quyen",
               new { controller = "Role", action = "AddRole" }
           );
            routes.MapRoute(
               "quan-ly-quyen/chinh-sua-quyen",
               "quan-ly-quyen/chinh-sua-quyen/{id}",
               new { controller = "Role", action = "AddRole", id = UrlParameter.Optional }
           );

            #endregion

            #region Customer

            routes.MapRoute(
               "quan-ly-khach-hang",
               "quan-ly-khach-hang",
               new { controller = "Customer", action = "Index" }
           );
            routes.MapRoute(
               "quan-ly-khach-hang/them-khach-hang",
               "quan-ly-khach-hang/them-khach-hang",
               new { controller = "Customer", action = "AddCustomer" }
           );
            routes.MapRoute(
               "quan-ly-khach-hang/chinh-sua-khach-hang",
               "quan-ly-khach-hang/chinh-sua-khach-hang/{id}",
               new { controller = "Customer", action = "AddCustomer", id = UrlParameter.Optional }
           );

            #endregion
            
            #region Customer

            routes.MapRoute(
               "quan-ly-cua-hang",
               "quan-ly-cua-hang",
               new { controller = "Store", action = "Index" }
           );
            routes.MapRoute(
               "quan-ly-cua-hang/them-cua-hang",
               "quan-ly-cua-hang/them-cua-hang",
               new { controller = "Store", action = "AddStore" }
           );
            routes.MapRoute(
               "quan-ly-cua-hang/chinh-sua-cua-hang",
               "quan-ly-cua-hang/chinh-sua-cua-hang/{id}",
               new { controller = "Store", action = "AddStore", id = UrlParameter.Optional }
           );

            #endregion

            #region income and expenditure patterns
            routes.MapRoute(
             "quan-ly-thu-chi/thu-hoat-dong",
             "quan-ly-thu-chi/thu-hoat-dong",
             new { controller = "IncomeAndExpense", action = "Index" }
          );
            routes.MapRoute(
               "quan-ly-thu-chi/chi-hoat-dong",
               "quan-ly-thu-chi/chi-hoat-dong",
               new { controller = "IncomeAndExpense", action = "Index2" }
            );
            #endregion

            #region capital
            routes.MapRoute(
               "quan-ly-nguon-von",
               "quan-ly-nguon-von",
               new { controller = "Capital", action = "Index" }
            );
            #endregion


            #region CashBook
            routes.MapRoute(
               "so-quy-tien-mat",
               "so-quy-tien-mat",
               new { controller = "CashBook", action = "Index" }
            );
            #endregion

            routes.MapRoute(
              "bat-ho",
              "bat-ho",
              new { controller = "Batho", action = "Index" }
           );

            routes.MapRoute(
            "vay-lai",
            "vay-lai",
            new { controller = "Pawn", action = "Index" }
         );
            routes.MapRoute(
            "401-error",
            "401-error",
            new { controller = "Home", action = "_401" }
         );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
