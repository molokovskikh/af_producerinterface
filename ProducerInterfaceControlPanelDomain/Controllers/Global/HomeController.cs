using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class HomeController : BaseController
	{

		/// <summary>
		/// Стартовая страница сайта, которая будет доступна после авторизации
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = new Statistics();

			model.ProducerCount = DB.AccountCompany.Count(x => x.ProducerId.HasValue && x.Account.Any());
			model.NotProducerCount = DB.AccountCompany.Count(x => !x.ProducerId.HasValue && x.Account.Any());

			model.ActionCount = DB.promotions.Count();
			model.AcceptedActionCount = DB.promotions.Count(x => x.Status == (byte)PromotionStatus.Confirmed);
			model.ActiveActionCount = DB.promotions.Count(x => x.Status == (byte)PromotionStatus.Confirmed && x.Enabled && x.Begin < DateTime.Now && x.End > DateTime.Now);

			var userByType					= DB.Account.GroupBy(x => x.Enabled).ToDictionary(x => x.Key, x => x.Count());
			model.UserCount					= userByType.Sum(x => x.Value);
			model.ActiveUserCount		= userByType.ContainsKey((int)UserStatus.Active) ? userByType[(int)UserStatus.Active] : 0;
			model.NewUserCount			= userByType.ContainsKey((int)UserStatus.New) ? userByType[(int)UserStatus.New] : 0;
			model.RequestUserCount	= userByType.ContainsKey((int)UserStatus.Request) ? userByType[(int)UserStatus.Request] : 0;
			model.BlockedUserCount	= userByType.ContainsKey((int)UserStatus.Blocked) ? userByType[(int)UserStatus.Blocked] : 0;

			model.ReportCount = DB.jobextend.Count();

			model.FeedBackNewCount = DB.AccountFeedBack.Count(x => x.Status == (int)FeedBackStatus.New);

			model.CatalogChangeRequest = DB.CatalogLog.Count(x => x.Apply == (sbyte)ApplyRedaction.New);

			return View(model);
		}

	}
}