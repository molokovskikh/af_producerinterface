using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc.Html;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
	public class FeedBackFunction
	{
		private producerinterface_Entities cntx_;

		public FeedBackFunction()
		{
			cntx_ = new producerinterface_Entities();
		}

		// инициализирует модель фильтра
		public FeedBackFilter GetFilter()
		{
			return new FeedBackFilter() {
				DateBegin = GetMinDate(),
				DateEnd = GetMaxDate(),
				PageIndex = 0,
				AccountId = 0,
				ProducerId = 0,
				ProducerList = GetProducerList(),
				AccountList = GetAccountList(),
				ItemsPerPageList = GetItemsPerPageList(),
				ItemsPerPage = 50
			};
		}

		// возвращает минимальную дату получения сообщений обратной связи
		private string GetMinDate()
		{
			var res = cntx_.AccountFeedBack.Min(x => x.DateAdd);
			return res.ToString("dd.MM.yyyy");
		}

		// возвращает максимальную дату получения сообщений обратной связи
		private string GetMaxDate()
		{
			var res = cntx_.AccountFeedBack.Max(x => x.DateAdd);
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
				Type				= modelDb.Type,
				StatusList = EnumHelper.GetSelectList(typeof(FeedBackStatus)).ToList()
			};

			return modelUi;
		}

		// возвращает коллекцию сообщений обратной связи с учетом фильтра
		private List<feedbackui> GetFeedBackFiltered(FeedBackFilter filter, ref int pageCount)
		{
			var dateBegin = DateTime.ParseExact(
				filter.DateBegin, "dd.MM.yyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None);
			var dateEnd = DateTime.ParseExact(
				filter.DateEnd, "dd.MM.yyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None).AddDays(1);

			var query = cntx_.feedbackui.Where(x => x.DateAdd >= dateBegin && x.DateAdd <= dateEnd);
			if (filter.AccountId != 0)
				query = query.Where(x => x.AccountId == filter.AccountId);
			if (filter.ProducerId != 0)
				query = query.Where(x => x.ProducerId == filter.ProducerId);

			var itemsCount = query.Count();
			if (itemsCount == 0)
				return new List<feedbackui>();

			var itemsToPage = filter.ItemsPerPage; //PagerCount(filter.PageCount);
			pageCount = (int)Math.Ceiling((decimal)itemsCount / itemsToPage);

			var cntxList = query.OrderByDescending(x => x.Id)
				.Skip(filter.PageIndex * itemsToPage)
				.Take(itemsToPage).ToList();

			return cntxList;
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

		private List<OptionElement> GetItemsPerPageList()
		{
			return new List<OptionElement>()
						{
								new OptionElement { Value = "20", Text = "20" },
								new OptionElement { Value = "50", Text = "50" },
								new OptionElement { Value = "100", Text = "100" },
								new OptionElement { Value = "1", Text = "1" }
						};
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

	}
}
