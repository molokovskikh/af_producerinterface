using System.Web;
using System.Web.Optimization;

namespace ProducerInterface
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
							.Include("~/Scripts/jquery-{version}.js")
							.Include("~/Scripts/globalize.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
							.Include("~/Scripts/jquery.validate.js",
								"~/Scripts/jquery.validate.unobtrusive.js",
								"~/Scripts/jquery.validate.globalize.js")
						);

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/MvcAjax").Include("~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/chosen")
                .Include(
                    "~/Scripts/Chosen/chosen.jquery.js",
                    "~/Scripts/jquery.maskedinput.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/WorkPlace")
                .Include(
                    "~/Scripts/WorkPlace/Analit.js",
                    "~/Scripts/WorkPlace/ZAnalit.js",
                    "~/Scripts/WorkPlace/ModalView.js"));

            bundles.Add(new ScriptBundle("~/bundles/cron")
                .Include(
                    "~/Content/Chosen/js/initCron.js",
                    "~/Content/Chosen/js/jqCron.js",
                    "~/Content/Chosen/js/jqCron.ru.js"));

            bundles.Add(new StyleBundle("~/Content/cron")
                .Include("~/Content/Chosen/css/jqCron.css"));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/MyStyle.css"));
        }
    }
}
