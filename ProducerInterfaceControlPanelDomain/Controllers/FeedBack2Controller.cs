using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class FeedBack2Controller : MasterBaseController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			TypeLoginUser = TypeUsers.ControlPanelUser;
			base.OnActionExecuting(filterContext);
		}

		public ActionResult Index(FeedBackFilter2 model)
		{
			if (!model.DateBegin.HasValue)
				model.DateBegin = GetMinDate();
			if (!model.DateEnd.HasValue)
				model.DateEnd = GetMaxDate();
			if (model.ItemsPerPage == 0)
				model.ItemsPerPage = 50;
			var prDictionary = GetProducerList();
			model.ProducerList = GetProducerListUi(prDictionary, model.ProducerId);
			model.AccountList = GetAccountList(model.AccountId);
			model.ItemsPerPageList = GetItemsPerPageList(model.ItemsPerPage);
			model.StatusList = GetStatusList(model.Status);
			return View(model);
		}

		/// <summary>
		/// Поиск сообщений обратной связи по фильтру
		/// </summary>
		/// <param name="filter">фильтр</param>
		/// <returns></returns>
		public ActionResult SearchResult(FeedBackFilter2 filter)
		{
			var query = DB.AccountFeedBack.AsQueryable();
			if (filter.DateBegin.HasValue)
				query = query.Where(x => x.DateAdd >= filter.DateBegin.Value);
			if (filter.DateEnd.HasValue) {
				var dateEnd = filter.DateEnd.Value.AddDays(1);
				query = query.Where(x => x.DateAdd <= dateEnd);
			}
			if (filter.AccountId.HasValue)
				query = query.Where(x => x.AccountId == filter.AccountId);
			if (filter.ProducerId.HasValue)
				query = query.Where(x => x.Account.AccountCompany.ProducerId == filter.ProducerId);
			if (filter.Status.HasValue)
				query = query.Where(x => x.Status == filter.Status);

			var itemsCount = query.Count();
			var info = new SortingPagingInfo() { CurrentPageIndex = filter.CurrentPageIndex, ItemsCount = itemsCount, ItemsPerPage = filter.ItemsPerPage };
			ViewBag.Info = info;
			ViewBag.PrDictionary = GetProducerList();

			var model = query.OrderByDescending(x => x.DateAdd).Skip(filter.CurrentPageIndex * filter.ItemsPerPage).Take(filter.ItemsPerPage).ToList();
			return View(model);
		}

		/// <summary>
		/// Форма ввода-вывода комментариев админов к сообщению обратной связи GET
		/// </summary>
		/// <param name="Id">идентификатор сообщения</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CommentToFeedBack(long Id)
		{
			var feedBack = DB.AccountFeedBack.Single(x => x.Id == Id);
			var model = new FeedBackComment() {
				Id = Id,
				Status = feedBack.Status,
				Description = feedBack.Description
			};
			model.StatusList = EnumHelper.GetSelectList(typeof(FeedBackStatus), (FeedBackStatus)model.Status).ToList();
			model.CommentList = DB.AccountFeedBackComment.Where(x => x.IdFeedBack == Id).OrderByDescending(x => x.DateAdd).ToList();
			return View(model);
		}

		/// <summary>
		/// Изменение статуса сообщения, добавление комментария
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CommentToFeedBack(FeedBackComment model)
		{
			var feedBack = DB.AccountFeedBack.Single(x => x.Id == model.Id);
			feedBack.AdminId = CurrentUser.Id;
			feedBack.DateEdit = DateTime.Now;
			feedBack.Status = model.Status;

			// если передан текст комментария
			if (!string.IsNullOrEmpty(model.Comment)) {
				var comment = new AccountFeedBackComment() {
					AdminId = CurrentUser.Id,
					DateAdd = DateTime.Now,
					Comment = model.Comment
				};
				feedBack.AccountFeedBackComment.Add(comment);
			}
			DB.SaveChanges();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Список статусов сообщений
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		private List<SelectListItem> GetStatusList(int? status = null)
		{
				return new List<SelectListItem> {
					new SelectListItem { Text = "Все обращения", Value = "" },
					new SelectListItem { Text = "Обработанные", Value = ((int)FeedBackStatus.Processed).ToString(), Selected = status == (int)FeedBackStatus.Processed },
					new SelectListItem { Text = "Необработанные", Value = ((int)FeedBackStatus.New).ToString(), Selected = status == (int)FeedBackStatus.New },
				};
		}

		/// <summary>
		/// Возвращает список производителей, от пользователей которых есть сообщения в обратной связи
		/// </summary>
		/// <returns></returns>
		private Dictionary<long, string> GetProducerList()
		{
			var cIds = DB.AccountFeedBack.Where(x => x.AccountId.HasValue).Select(x => x.Account.AccountCompany.Id).Distinct().ToList();
			var pIds = DB.AccountCompany.Where(x => cIds.Contains(x.Id) && x.ProducerId.HasValue).Select(x => x.ProducerId).Distinct().ToList();
			var plDictionary = DB.producernames
				.Where(x => pIds.Contains(x.ProducerId))
				.OrderBy(x => x.ProducerName)
				.ToDictionary(x => x.ProducerId, x => x.ProducerName);
			return plDictionary;
		}

		/// <summary>
		/// Возвращает список производителей, от пользователей которых есть сообщения в обратной связи
		/// </summary>
		/// <param name="prDictionary"></param>
		/// <param name="producerId"></param>
		/// <returns></returns>
		private List<SelectListItem> GetProducerListUi(Dictionary<long, string> prDictionary, long? producerId = null)
		{
			var producerList = new List<SelectListItem>() { new SelectListItem { Text = "Выберите производителя", Value = "" } };
			producerList.AddRange(prDictionary.Select(x => new SelectListItem { Text = x.Value, Value = x.Key.ToString(), Selected = producerId == x.Key }));
			return producerList;
		}

		/// <summary>
		/// Возвращает минимальную дату получения сообщений обратной связи
		/// </summary>
		/// <returns></returns>
		private DateTime? GetMinDate()
		{
			if (DB.AccountFeedBack.Any())
				return DB.AccountFeedBack.Min(x => x.DateAdd);
			return null;
		}

		/// <summary>
		/// Возвращает максимальную дату получения сообщений обратной связи
		/// </summary>
		/// <returns></returns>
		private DateTime? GetMaxDate()
		{
			if (DB.AccountFeedBack.Any())
				return DB.AccountFeedBack.Max(x => x.DateAdd);
			return null;
		}

		/// <summary>
		/// Список возможных значений элементов на странице
		/// </summary>
		/// <param name="itemsPerPage"></param>
		/// <returns></returns>
		private List<SelectListItem> GetItemsPerPageList(int itemsPerPage)
		{
			return new List<SelectListItem>()
				{
						new SelectListItem { Value = "20", Text = "20", Selected = itemsPerPage == 20},
						new SelectListItem { Value = "50", Text = "50", Selected = itemsPerPage == 50 },
						new SelectListItem { Value = "100", Text = "100", Selected = itemsPerPage == 100 },
						new SelectListItem { Value = "1", Text = "1", Selected = itemsPerPage == 50 }
				};
		}

		/// <summary>
		/// Возвращает список пользователей, от которых есть сообщения в обратной связи
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		private List<SelectListItem> GetAccountList(long? accountId = null)
		{
			var accIds = DB.AccountFeedBack.Select(x => x.AccountId).Distinct().ToList();
			var acc = DB.Account
				.Where(x => accIds.Contains(x.Id))
				.Select(x => new SelectListItem { Text = x.Login + " " + x.Name, Value = x.Id.ToString(), Selected = accountId == x.Id })
				.OrderBy(x => x.Text).ToList();

			var accountList = new List<SelectListItem>() { new SelectListItem { Value = "", Text = "Выберите пользователя" } };
			accountList.AddRange(acc);
			return accountList;
		}
	}
}