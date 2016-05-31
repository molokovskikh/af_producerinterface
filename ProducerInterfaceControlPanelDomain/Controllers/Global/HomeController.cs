using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class HomeController : MasterBaseController
	{

		/// <summary>
		/// Стартовая страница сайта, которая будет доступна после авторизации
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = new Statistics();

			model.ProducerCount = cntx_.AccountCompany.Count(x => x.ProducerId.HasValue && x.Account.Any());
			model.NotProducerCount = cntx_.AccountCompany.Count(x => !x.ProducerId.HasValue && x.Account.Any());

			model.ActionCount = cntx_.promotions.Count();
			model.AcceptedActionCount = cntx_.promotions.Count(x => x.Status);
			model.ActiveActionCount = cntx_.promotions.Count(x => x.Status && x.Enabled && x.Begin <= DateTime.Now && x.End > DateTime.Now);

			var userByType					= cntx_.Account.GroupBy(x => x.Enabled).ToDictionary(x => x.Key, x => x.Count());
			model.UserCount					= userByType.Sum(x => x.Value);
			model.ActiveUserCount		= userByType.ContainsKey((int)UserStatus.Active) ? userByType[(int)UserStatus.Active] : 0;
			model.NewUserCount			= userByType.ContainsKey((int)UserStatus.New) ? userByType[(int)UserStatus.New] : 0;
			model.RequestUserCount	= userByType.ContainsKey((int)UserStatus.Request) ? userByType[(int)UserStatus.Request] : 0;
			model.BlockedUserCount	= userByType.ContainsKey((int)UserStatus.Blocked) ? userByType[(int)UserStatus.Blocked] : 0;

			model.ReportCount = cntx_.jobextend.Count();

			model.FeedBackNewCount = cntx_.AccountFeedBack.Count(x => x.Status == (int)FeedBackStatus.New);

			return View(model);
		}

	}
}