using System.Web.Optimization;

namespace ProducerInterfaceControlPanelDomain
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
				"~/Scripts/jquery-1.10.2.min.js",
				"~/Scripts/jquery.unobtrusive-ajax.min.js",
				"~/Scripts/jquery.globalize/*.js",
				"~/Scripts/jquery.globalize/cultures/*.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval")
				.Include("~/Scripts/jquery.validate.js",
					"~/Scripts/jquery.validate.unobtrusive.js",
					"~/Scripts/jquery.validate.globalize.js")
				);

			bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
				"~/Scripts/bootstrap-datepicker.js",
				"~/Scripts/locales/bootstrap-datepicker.ru.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
				"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/respond.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/bootstrap.css",
				"~/Content/bootstrap-datepicker.min.css",
				"~/Content/default.css",
				"~/Content/Chosen/css/Site.css",
				"~/Content/site.css"));


			bundles.Add(new ScriptBundle("~/bundles/chosen")
				.Include(
					"~/Content/Chosen/js/chosen.jquery.js",
					"~/Scripts/jquery.maskedinput.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/cron")
				.Include("~/Content/Chosen/js/initCron.js",
					"~/Content/Chosen/js/jqCron.js",
					"~/Content/Chosen/js/jqCron.ru.js"));

			bundles.Add(new StyleBundle("~/Content/cron")
				.Include("~/Content/Chosen/css/jqCron.css"));
		}
	}
}