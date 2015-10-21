using System.Web;
using System.Web.Optimization;

namespace KMBit
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                      "~/Scripts/jquery-ui-1.11.4.custom/jquery-ui.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",                      
                      "~/Scripts/respond.js",
                      "~/Scripts/bootstrap-datepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/sitejs").Include(
                       "~/Scripts/kmsite.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-datepicker.min.css",
                      "~/Scripts/jqPlot/jquery.jqplot.min.css",
                      "~/Scripts/jquery-ui-1.11.4.custom/jquery-ui.min.css",
                      //"~/Scripts/kindeditor/themes/default/default.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqPlot").Include(
                        "~/Scripts/jqPlot/jquery.jqplot.min.js",
                        "~/Scripts/jqPlot/plugins/jqplot.pieRenderer.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/kindEditor").Include(
                       "~/Scripts/kindeditor/kindeditor-all-min.js"));
        }
    }
}
