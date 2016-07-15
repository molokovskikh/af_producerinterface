using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterfaceControlPanelDomain
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new ErrorFilter());
		}
	}
}
