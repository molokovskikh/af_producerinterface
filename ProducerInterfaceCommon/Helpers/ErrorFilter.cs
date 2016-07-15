using System.Web.Mvc;
using log4net;

namespace ProducerInterfaceCommon.Helpers
{
	public class ErrorFilter : HandleErrorAttribute
	{
		public ILog log = LogManager.GetLogger(typeof(ErrorFilter));

		public override void OnException(ExceptionContext filterContext)
		{
			log.Error("������ ��� ���������� �������", filterContext.Exception);
			base.OnException(filterContext);
		}
	}
}