using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
	public class FeedBackList
	{
		public List<feedbackui> FeedList { get; set; }
		public List<Paginator> PaginatorLinks { get; set; }

		public int PageIndex { get; set; }

		public string SortTime { get; set; }
		public string SortProducerName { get; set; }
		public string SortAccountName { get; set; }
		public string SortType { get; set; }
		public string SortStatus { get; set; }
	}

	public class FeedBackItemSelect : AccountFeedBack
	{
		public List<SelectListItem> StatusList { get; set; }
	}

	// данная модель будет возвращатся при поиске результатов с клиента
	public class FeedBackFilter2
	{
		[Display(Name = "Статус")]
		public int? Status { get; set; }

		[Display(Name = "Производитель")]
		public long? ProducerId { get; set; }

		[Display(Name = "Пользователь")]
		public long? AccountId { get; set; }

		[Display(Name = "От")]
		[UIHint("Date")]
		public DateTime? DateBegin { get; set; }

		[Display(Name = "До")]
		[UIHint("Date")]
		public DateTime? DateEnd { get; set; }

		public int CurrentPageIndex { get; set; }

		[Display(Name = "Количество записей на странице")]
		public int ItemsPerPage { get; set; }

		public List<SelectListItem> StatusList { get; set; }
		public List<SelectListItem> ProducerList { get; set; }
		public List<SelectListItem> AccountList { get; set; }
		public List<SelectListItem> ItemsPerPageList { get; set; }
	}

	// данная модель будет возвращатся при поиске результатов с клиента
	public class FeedBackComment
	{
		[Display(Name = "Сообщение #")]
		public long Id { get; set; }

		[Display(Name = "Текст обращения")]
		public string Description { get; set; }

		[Display(Name = "Статус")]
		public sbyte Status { get; set; }

		[Display(Name = "Комментарий")]
		public string Comment { get; set; }

		public List<SelectListItem> StatusList { get; set; }

		public IEnumerable<AccountFeedBackComment> CommentList { get; set; }
	}


	// данная модель будет возвращатся при поиске результатов с клиента
	public class FeedBackFilter
	{
		public string DateBegin { get; set; }
		public string DateEnd { get; set; }
		public int PageIndex { get; set; }
		public long ProducerId { get; set; }
		public long AccountId { get; set; }
		public int ItemsPerPage { get; set; }

		public List<OptionElement> ProducerList { get; set; }
		public List<OptionElement> AccountList { get; set; }
		public List<OptionElement> ItemsPerPageList { get; set; }
	}


	public class Paginator
	{
		public int Counter { get; set; }
		public int ViewCounter { get; set; }
		public string ClassName { get; set; }
	}

}
