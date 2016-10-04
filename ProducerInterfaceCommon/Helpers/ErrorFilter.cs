using System.Web.Mvc;
using log4net;

namespace ProducerInterfaceCommon.Helpers
{
	public class ErrorFilter : HandleErrorAttribute
	{
		public ILog log = LogManager.GetLogger(typeof(ErrorFilter));

		public override void OnException(ExceptionContext filterContext)
		{
			ThreadContext.Properties["url"] = filterContext.HttpContext.Request.Url;
			log.Error("������ ��� ���������� �������", filterContext.Exception);
			base.OnException(filterContext);
		}
	}
}