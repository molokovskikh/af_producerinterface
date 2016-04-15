using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
	public class FeedBackFunction
	{
		private producerinterface_Entities cntx_;
		private Account CurrentUser;
		NamesHelper h;

		public FeedBackFunction(Account currentUser)
		{
			cntx_ = new producerinterface_Entities();
			CurrentUser = currentUser;
			h = new NamesHelper(cntx_, CurrentUser.Id);
		}

		// инициализирует модель фильтра
		public FeedBackFilter GetFilter()
		{
			var filter = new FeedBackFilter();
			filter.DateBegin = GetMinDate();
			filter.DateEnd = GetMaxDate();
			filter.PageIndex = 0;
			filter.AccountId = 0;
			filter.ProducerId = 0;
			filter.ProducerList = GetProducerList();
			filter.AccountList = GetAccountList();
			filter.PageCountList = GetItemsToPage();
			filter.PageCount = 1;

			return filter;
		}

		// возвращает минимальную дату получения сообщений обратной связи
		private string GetMinDate()
		{
			var res = DateTime.Now;
			var dt = cntx_.AccountFeedBack.Min(x => x.DateAdd);
			if (dt.HasValue)
				res = dt.Value;
			return res.ToString("dd.MM.yyyy");
		}

		// возвращает максимальную дату получения сообщений обратной связи
		private string GetMaxDate()
		{
			var res = DateTime.Now;
			var dt = cntx_.AccountFeedBack.Max(x => x.DateAdd);
			if (dt.HasValue)
				res = dt.Value.AddDays(1);
			return res.ToString("dd.MM.yyyy");
		}

		public FeedBackList GetFeedBackList(FeedBackFilter feedBackFilter)
		{
			var result = new FeedBackList();
			var pageCount = 0;

			result.FeedList = GetFeedBackFiltered(feedBackFilter, ref pageCount);
			if (!result.FeedList.Any())
				return result;

			result.PageIndex = feedBackFilter.PageIndex;
			result.PaginatorLinks = GetPaginator(pageCount, feedBackFilter.PageIndex);
			SetViewFilter(ref result, feedBackFilter);

			return result;
		}

		// возвращает один элемент
		public FeedBackItemSelect GetOneFeedBack(int Id)
		{
			var modelDb = cntx_.AccountFeedBack.Single(x => x.Id == Id);
			var modelUi = new FeedBackItemSelect() {
				Id					= modelDb.Id,
				Description = modelDb.Description,
				Status			= modelDb.Status,
				Comment			= modelDb.Comment,
				Type				= modelDb.Type,
				StatusList = EnumHelper.GetSelectList(typeof(FeedBackStatus), modelDb.StatusEnum).ToList()
			};

			return modelUi;
		}

		// возвращает коллекцию сообщений обратной связи с учетом фильтра
		private List<FeedBackItem> GetFeedBackFiltered(FeedBackFilter filter, ref int pageCount)
		{
			var ret = new List<FeedBackItem>();

			var dateBegin = Convert.ToDateTime(filter.DateBegin);
			var dateEnd = Convert.ToDateTime(filter.DateEnd);

			var query = cntx_.AccountFeedBack.Where(x => x.DateAdd >= dateBegin && x.DateAdd <= dateEnd);
			if (filter.AccountId != 0)
				query = query.Where(x => x.AccountId == filter.AccountId);
			if (filter.ProducerId != 0)
				query = query.Where(x => x.Account != null && x.Account.AccountCompany != null && x.Account.AccountCompany.ProducerId == filter.ProducerId);

			var itemsCount = query.Count();
			if (itemsCount == 0)
				return ret;

			var itemsToPage = PagerCount(filter.PageCount);
			pageCount = (int)Math.Ceiling((decimal)itemsCount / itemsToPage);

			var cntxList = query.OrderByDescending(x => x.Id)
				.Skip(filter.PageIndex * itemsToPage)
				.Take(itemsToPage).ToList();

			// переложили из модели БД в модель для UI
			MapDbToUi(ref ret, cntxList);

			return ret;
		}

		private void SetViewFilter(ref FeedBackList ModelView, FeedBackFilter filter = null)
		{
			ModelView.SortTime = "block";
			ModelView.SortProducerName = "none";
			ModelView.SortAccountName = "none";
			ModelView.SortType = "none";
			ModelView.SortStatus = "none";

			if (filter == null) // если фильтр не был передан, по умолчанию фильтруется по дате от начала к концу
			{
				ModelView.SortTime = "block";
			}
			else
			{
				if (filter.AccountId != 0)
				{ ModelView.SortAccountName = "block"; }


				if (filter.ProducerId != 0)
				{ ModelView.SortProducerName = "block"; }

			}
		}

		// 
		private void MapDbToUi(ref List<FeedBackItem> modelUi, List<AccountFeedBack> modelDb)
		{
			modelUi = modelDb.Select(x => new FeedBackItem
			{
				About = x.Contacts,
				AccountId = x.AccountId,
				CreateDate = (DateTime)x.DateAdd,
				Description = x.Description,
				FeedBackStatus = x.StatusEnum,
				TypeFeedBack = (int)x.TypeEnum,
				Id = x.Id,
				UrlString = x.UrlString,
				AccountLogin = x.Account?.Login,
				Producername = GetProducerName(x)
			}).ToList();
		}

		// возвращает список пользователей, от которых есть сообщения в обратной связи
		private List<OptionElement> GetAccountList()
		{
			var accountList = new List<OptionElement>() { new OptionElement { Value = "0", Text = "Выберите пользователя" } };
			var accIds = cntx_.AccountFeedBack.Select(x => x.AccountId).Distinct().ToList();
			var acc = cntx_.Account
				.Where(x => accIds.Contains(x.Id))
				.Select(x => new OptionElement { Text = x.Login + " " + x.Name, Value = x.Id.ToString() })
				.OrderBy(x => x.Text).ToList();

			accountList.AddRange(acc);
			return accountList;
		}

		private List<OptionElement> GetItemsToPage()
		{
			return new List<OptionElement>()
						{
								new OptionElement { Value = "0", Text = "20" },
								new OptionElement { Value = "1", Text = "50" },
								new OptionElement { Value = "2", Text = "100" },
								new OptionElement { Value = "3", Text = "1" }
						};
		}

		// возвращает количество элементов на страницу в зависимости от номера выбранной опции TODO: удалить
		private int PagerCount(int value)
		{
			var ret = GetItemsToPage().Single(x => x.Value == value.ToString()).Text;
			return Convert.ToInt32(ret);
		}

		// возвращает список производителей, от пользователей которых есть сообщения в обратной связи
		private List<OptionElement> GetProducerList()
		{
			var cIds = cntx_.AccountFeedBack.Where(x => x.AccountId.HasValue).Select(x => x.Account.AccountCompany.Id).Distinct().ToList();
			var pIds = cntx_.AccountCompany.Where(x => cIds.Contains(x.Id)).Select(x => x.ProducerId).Distinct().ToList();
			var pr = cntx_.producernames
				.Where(x => pIds.Contains(x.ProducerId))
				.Select(x => new OptionElement() { Text = x.ProducerName, Value = x.ProducerId.ToString() })
				.OrderBy(x => x.Text).ToList();

			var producerList = new List<OptionElement>() { new OptionElement { Text = "Выберите производителя", Value = "0" } };
			producerList.AddRange(pr);

			return producerList;
		}

		private List<Paginator> GetPaginator(int pageCount, int pageIndex)
		{
			var pag = new List<Paginator>();

			if (pageCount < 10)
			{
				for (var i = 0; i < pageCount; i++)
				{
					var pagAdd = new Paginator { Counter = i, ViewCounter = i + 1 };
					if (i == pageIndex)
						pagAdd.ClassName = "active primary";
					pag.Add(pagAdd);
				}
			}
			else {
				if (pageIndex <= 3)
				{
					for (var i = 0; i < 10; i++)
					{
						var pagAdd = new Paginator { Counter = i, ViewCounter = i + 1 };
						if (i == pageIndex)
							pagAdd.ClassName = "active primary";
						pag.Add(pagAdd);
					}
				}
			else {
					var currentPageLocal = pageIndex - 3;
					int off = 0;
					for (var i = currentPageLocal; i < pageCount; i++)
					{
						off++;
						var pagAdd = new Paginator { Counter = i, ViewCounter = i + 1 };
						if (i == pageIndex)
							pagAdd.ClassName = "active primary";
						pag.Add(pagAdd);
						if (off > 10)
							break;
					}
				}
			}
			return pag;
		}

		private string GetProducerName(AccountFeedBack feedaccount)
		{
			if (feedaccount.Account == null || feedaccount.AccountId == 0)
			{
				return "";
			}
			if (feedaccount.Account.AccountCompany.ProducerId == null)
			{
				return "";
			}
			return cntx_.producernames.Where(x => x.ProducerId == feedaccount.Account.AccountCompany.ProducerId).First().ProducerName;
		}
	}
}
