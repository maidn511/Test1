using System.Web;
using System.Web.Optimization;

namespace Pawn
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // AngularJS
            bundles.Add(new ScriptBundle("~/bundles/js/angular").Include(
                                         "~/Scripts/angular/angular.js",
                                         "~/Scripts/angular/angular-checklist-model.js",
                                         "~/Scripts/angular/angular-route.js",
                                         "~/Scripts/angular/ng-websocket.js",
                                         "~/Scripts/angular/angular-animate.js",
                                         "~/Scripts/angular/angular-touch.js",
                                         "~/Scripts/angular/chosen.jquery.min.js",
                                         "~/Scripts/angular/angular-chosen.min.js",
                                         "~/Scripts/angular/angular.unobtrusive.validation.min.js",
                                         "~/Scripts/angular/angular.unobtrusive.validation.tpls.min.js",
                                         "~/Scripts/angular/angular-sanitize.js"));
            //bootstrap css
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      //"~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //widgets css
            bundles.Add(new StyleBundle("~/bundles/css/widgets").Include(
                       "~/Content/widgets/chosen/chosen.css",
                       "~/Content/widgets/bootstrap-toggle/angular-bootstrap-toggle.min.css",
                       "~/Content/widgets/jgrowl-notifications/jgrowl.css",
                        "~/Content/css/style-dev.css"
                       ));

            //bootstrap js
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      //"~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            // widgets Js
            bundles.Add(new ScriptBundle("~/bundles/js/widgets").Include(
                      "~/Content/widgets/jgrowl-notifications/jgrowl.js",
                      "~/Content/widgets/bootstrap-toggle/angular-bootstrap-toggle.js",
                      "~/Content/widgets/chosen/chosen.jquery.min.js",
                      "~/Content/widgets/chosen/chosen.proto.min.js"));

            BundleTable.EnableOptimizations = true;

        }
    }
}
