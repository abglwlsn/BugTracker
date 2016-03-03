using System.Web;
using System.Web.Optimization;

namespace BugTracker
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                                                "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jqueryui-1.11.4.min.js",
                        "~/Site Template/js/jquery.slimscroll.min.js",
                        "~/Site Template/js/classie.js",
                        "~/Site Template/js/sortable.min.js",
                        "~/Site Template/js/bootstrap-select.min.js",
                        "~/Site Template/js/summernote.min.js",
                        "~/Site Template/js/jquery.magnific-popup.min.js",
                        "~/Site Template/js/bootstrap.file-input.js",
                        "~/Site Template/js/bootstrap-datepicker.js",
                        "~/Site Template/js/ickeck.min.js",
                        "~/Site Template/js/jquery.snippet.js",
                        "~/Site Template/js/jquery.easyWizard.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                                            "~/Content/bootstrap.min.css",
                      "~/Site Template/css/style.css",
                      "~/Site Template/css/style-responsive.css",
                      "~/Site Template/css/animate.css",
                      "~/Site Template/css/morris.css",
                      "~/Site Template/css/component.css",
                      "~/Site Template/css/sortable-theme-bootstrap.css",
                      "~/Site Template/css/green.css",
                      "~/Site Template/css/bootstrap-select.min.css",
                      "~/Site Template/css/summernote.css",
                      "~/Site Template/css/magnific-popup.css",
                      "~/Site Template/css/datepicker.css"));
        }
    }
}
