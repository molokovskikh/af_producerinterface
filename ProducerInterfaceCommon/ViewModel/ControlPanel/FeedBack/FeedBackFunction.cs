using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public FeedBackFilter GetFilter()
		{
			FeedBackFilter FBF = new FeedBackFilter();
			FBF.DateBegin = DateTime.Now.AddMonths(-1).ToString("dd.MM.yyyy");
			FBF.DateEnd = DateTime.Now.AddDays(1).ToString("dd.MM.yyyy");
			FBF.PageIndex = 0;
			FBF.AccountId = 0;
			FBF.ProducerId = 0;
			FBF.ProducerList = GetProducerList(h);
			FBF.AccountList = GetAccountList(h);
			FBF.PageCountList = GetPageCountList();
			FBF.PageCount = 1;

			return FBF;
		}

		public FeedBackList GetFeedBackList(FeedBackFilter feedBackFilter)
		{
			FeedBackList FBL_result = new FeedBackList();

			int MaxCount = 0;

			FBL_result.FeedList = GetFeedBackListItems(feedBackFilter, ref MaxCount);

			if (FBL_result.FeedList.Count() == 0)
			{
				return FBL_result;
			}

			FBL_result.PageIndex = feedBackFilter.PageIndex;
			FBL_result.PaginatorLinks = GetPaginator(MaxCount, feedBackFilter.PageIndex);
			SetViewFilter(ref FBL_result, feedBackFilter);

			return FBL_result;
		}

		public FeedBackItemSelect GetOneFeedBack(int Id)
		{
			var cntx_FeedBackItem = cntx_.AccountFeedBack.Where(x => x.Id == Id).ToList();
			var ListFeedBack = new List<FeedBackItem>();
			AccountFeedBackToFeedBackViewModelConverter(ref ListFeedBack, cntx_FeedBackItem);

			FeedBackItemSelect FeedBackModelView = new FeedBackItemSelect();

			FeedBackModelView = (FeedBackItemSelect)ListFeedBack.Select(x => new FeedBackItemSelect
			{
				About = x.About,
				AccountId = x.AccountId,
				CreateDate = x.CreateDate,
				Id = x.Id,
				Description = x.Description,
				Message = x.Message,
				Producername = x.Producername,
				TypeFeedBack = x.TypeFeedBack,
				AccountLogin = x.AccountLogin,
				UrlString = x.UrlString,
				FeedBackStatus = x.FeedBackStatus
			}).First();

			//var FeedBackCommentItems = new List<FeedBackComment>();

			//foreach (var CommentItem in cntx_FeedBackItem.First().AccountFeedBackComment)
			//{
			//    FeedBackCommentItems.Add(new FeedBackComment { AdminName = cntx_.Account.Where(x => x.Id == CommentItem.AdminId).First().Login, Id = CommentItem.Id, DateAdd = (DateTime)CommentItem.DateAdd, Description = CommentItem.Comment, IdAccount = CommentItem.AdminId });
			//}

			//FeedBackModelView.Comments = FeedBackCommentItems;

			FeedBackModelView.FeedStatusId = cntx_FeedBackItem.First().Status;

			List<OptionElement> StatusList = Enum.GetValues(typeof(Enums.FeedBackStatus)).Cast<Enums.FeedBackStatus>().ToList().Select(x =>
			new OptionElement
			{
				Text = ProducerInterfaceCommon.Heap.AttributeHelper.DisplayName(x),
				Value = ((int)x).ToString()
			}).ToList();

			FeedBackModelView.StatusList = StatusList;

			return FeedBackModelView;

		}

		private List<FeedBackItem> GetFeedBackListItems(FeedBackFilter feedBackFilter, ref int MaxCount)
		{
			var ret = new List<FeedBackItem>();

			var DateBegin = Convert.ToDateTime(feedBackFilter.DateBegin);
			var DateEnd = Convert.ToDateTime(feedBackFilter.DateEnd);

			var cntx_list = cntx_.AccountFeedBack.Where(x => x.DateAdd >= DateBegin && x.DateAdd <= DateEnd).ToList().OrderByDescending(x => x.Id).ToList();

			if (cntx_list == null || cntx_list.Count() == 0)
			{
				return ret;
			}

			if (feedBackFilter.AccountId != 0)
			{
				cntx_list = cntx_list.Where(x => x.AccountId == feedBackFilter.AccountId).ToList();
			}

			if (cntx_list == null || cntx_list.Count() == 0)
			{
				return ret;
			}

			if (feedBackFilter.ProducerId != 0)
			{
				cntx_list = cntx_list.Where(x => x.Account != null).Where(x => x.Account.AccountCompany != null).Where(x => x.Account.AccountCompany.ProducerId != null)
						.Where(x => x.Account.AccountCompany.ProducerId == feedBackFilter.ProducerId).ToList();
			}

			MaxCount = cntx_list.Count();
			MaxCount = (int)Math.Ceiling((decimal)MaxCount / PagerCount(feedBackFilter.PageCount));

			cntx_list = cntx_list.Skip(feedBackFilter.PageIndex * PagerCount(feedBackFilter.PageCount)).ToList().Take(PagerCount(feedBackFilter.PageCount)).ToList();

			AccountFeedBackToFeedBackViewModelConverter(ref ret, cntx_list);

			return ret;
		}


		public FeedBackList GetFeedBackFilterView(Heap.NamesHelper h, FeedBackFilter FeedFilter)
		{
			var ModelView = new List<FeedBackItem>(); /* лист обращений */

			int MaxListCount = 0;    /*  количество сторок по данному фильтру необходимо для посторение Пагинатора */

			ModelView = GetListFeedBackModel(ref MaxListCount, FeedFilter); /* заполняем лист обращений по фильтру */
			if (ModelView == null || ModelView.Count() == 0)
			{
				return new FeedBackList();  /* если обращений по данному фильтру не найдено, возвращаем незаполненую модель для передачи на клиент сообщения об неуспешности фильтрации */
			}

			var ReturnModel = new FeedBackList();  /* Модель отправляемая клиенту */

			SetViewFilter(ref ReturnModel, FeedFilter);  /* отображение по каким столбцам фильтруется таблица */



			ReturnModel.PaginatorLinks = GetPaginator(MaxListCount, FeedFilter.PageIndex); /* заполняем пагинатор */

			ReturnModel.PageIndex = FeedFilter.PageIndex; /* текущий индех пагинатора */



			int MaxPager = (int)Math.Ceiling((double)(Convert.ToInt32(GetPageCountList().Where(x => x.Value == FeedFilter.PageCountList[FeedFilter.PageCount].Value).First().Value) / 50));

			return ReturnModel;
		}

		private void SetViewFilter(ref FeedBackList ModelView, FeedBackFilter FeedFilter = null)
		{
			ModelView.SortTime = "block";
			ModelView.SortProducerName = "none";
			ModelView.SortAccountName = "none";
			ModelView.SortType = "none";
			ModelView.SortStatus = "none";

			if (FeedFilter == null) // если фильтр не был передан, по умолчанию фильтруется по дате от начала к концу
			{
				ModelView.SortTime = "block";
			}
			else
			{
				if (FeedFilter.AccountId != 0)
				{ ModelView.SortAccountName = "block"; }


				if (FeedFilter.ProducerId != 0)
				{ ModelView.SortProducerName = "block"; }

			}
		}


		private List<FeedBackItem> GetListFeedBackModel(ref int MaxListCount, FeedBackFilter FeedFilter)
		{
			List<FeedBackItem> ret = new List<FeedBackItem>();
			var AccountFeedBackList = GetFeedBackList(ref MaxListCount, FeedFilter);

			if (AccountFeedBackList == null || AccountFeedBackList.Count() == 0)
			{
				return ret;
			}

			AccountFeedBackToFeedBackViewModelConverter(ref ret, AccountFeedBackList);

			return ret;
		}

		private void AccountFeedBackToFeedBackViewModelConverter(ref List<FeedBackItem> FB_ViewModel, List<AccountFeedBack> Account_FB_List)
		{
			FB_ViewModel = Account_FB_List.Select(x => new FeedBackItem
			{
				About = x.Contacts,
				AccountId = x.AccountId,
				CreateDate = (DateTime)x.DateAdd,
				Description = x.Description,
				FeedBackStatus = (Enums.FeedBackStatus)x.Status,
				TypeFeedBack = (int)(FeedBackType)x.Type,
				Id = x.Id,
				UrlString = x.UrlString,
				AccountLogin = x.Account?.Login,
				Producername = GetProducerName(x)
			}).ToList();
		}

		private List<OptionElement> GetAccountList(Heap.NamesHelper h)
		{
			var AccountList = new List<OptionElement>() { new OptionElement { Value = "0", Text = "Выберите пользователя" } };
			AccountList.AddRange(cntx_.Account.Where(x => x.TypeUser == (sbyte)TypeUsers.ProducerUser && x.Enabled == 1).ToList().Select(x => new OptionElement { Text = x.Login + " " + x.Name, Value = x.Id.ToString() }).ToList());

			return AccountList;
		}

		private List<OptionElement> GetPageCountList()
		{
			return new List<OptionElement>()
						{
								new OptionElement { Value = "0", Text = "20" },
								new OptionElement { Value = "1", Text = "50" },
								new OptionElement { Value = "2", Text = "100" },
								new OptionElement { Value = "3", Text = "1" }
						};
		}

		private int PagerCount(int Value)
		{
			var ret = GetPageCountList().Where(x => x.Value == Value.ToString()).First().Text;
			return Convert.ToInt32(ret);
		}

		private List<OptionElement> GetProducerList(NamesHelper h)
		{
			var ProducerList = new List<OptionElement>() { new OptionElement
						{
								 Text = "Выберите производителя", Value = "0"
						} };

			ProducerList.AddRange(h.RegisterListProducer().ToList());

			return ProducerList;
		}

		private List<AccountFeedBack> GetFeedBackList(ref int MaxCount, FeedBackFilter feedFilter = null)
		{
			if (feedFilter == null)
			{
				MaxCount = cntx_.AccountFeedBack.Count();
				return cntx_.AccountFeedBack.OrderByDescending(x => x.DateAdd).Take(100).ToList();
			}
			else
			{

				DateTime FeedDateBegin = Convert.ToDateTime(feedFilter.DateBegin);
				DateTime FeedDateEnd = Convert.ToDateTime(feedFilter.DateEnd);

				var ret = cntx_.AccountFeedBack.Where(xx => xx.DateAdd >= FeedDateBegin && xx.DateAdd <= FeedDateEnd).ToList();

				if (ret == null || ret.Count() == 0)
				{
					return ret;
				}
				if (feedFilter.ProducerId != 0)
				{
					ret = ret.Where(x => x.Account != null).ToList().Where(xx => xx.Account.AccountCompany.ProducerId != null).ToList().Where(xx => xx.Account.AccountCompany.ProducerId == feedFilter.ProducerId).ToList();
				}
				if (ret == null || ret.Count() == 0)
				{
					return ret;
				}
				if (feedFilter.AccountId != 0)
				{
					ret = ret.Where(xx => xx.Account != null).Where(xx => xx.AccountId == feedFilter.AccountId).ToList();
				}
				if (ret == null || ret.Count() == 0)
				{
					return ret;
				}

				var PageCountList = GetPageCountList();

				//    var OnePageListLeght = Convert.ToInt32(PageCountList.Where(x => x.Id == feedFilter.Pager).First().Name);

				ret = ret.OrderByDescending(x => x.Id).ToList();

				//if (ret.Count() <= OnePageListLeght)
				//{
				//    return ret;
				//}
				//if (ret.Count() > OnePageListLeght)
				//{
				//    MaxCount = ret.Count();                 
				//    ret = ret.ToList().Skip((OnePageListLeght)* feedFilter.PageIndex).Take(OnePageListLeght).ToList();                  
				//}

				return ret;
			}
		}

		private List<Paginator> GetPaginator(int PageMax, int CurrentPage)
		{
			List<Paginator> Pag = new List<Paginator>();

			if (PageMax < 10)
			{
				for (var X = 0; X < PageMax; X++)
				{
					var Y = X + 1;
					if (X == CurrentPage)
					{
						var PagAdd = new Paginator { Counter = X, ViewCounter = Y, ClassName = "active primary" };
						Pag.Add(PagAdd);
					}
					else
					{
						var PagAdd = new Paginator { Counter = X, ViewCounter = Y };
						Pag.Add(PagAdd);
					}
				}
			}

			if (PageMax >= 10)
			{
				if (CurrentPage < 3)
				{
					for (var X = 0; X < 10; X++)
					{
						var Y = X + 1;
						if (X == CurrentPage)
						{
							var PagAdd = new Paginator { Counter = X, ViewCounter = Y, ClassName = "active primary" };
							Pag.Add(PagAdd);
						}
						else
						{
							var PagAdd = new Paginator { Counter = X, ViewCounter = Y };
							Pag.Add(PagAdd);
						}
					}
				}

				if (CurrentPage > 3)
				{
					var CurrentPageLocal = CurrentPage - 3;
					int Off = 0;
					for (var X = CurrentPageLocal; X < PageMax; X++)
					{
						Off++;
						var Y = X + 1;
						if (X == CurrentPage)
						{
							var PagAdd = new Paginator { Counter = X, ViewCounter = Y, ClassName = "active primary" };
							Pag.Add(PagAdd);
						}
						else
						{
							var PagAdd = new Paginator { Counter = X, ViewCounter = Y };
							Pag.Add(PagAdd);
						}
						if (Off > 10)
						{
							break;
						}
					}
				}
			}
			return Pag;
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
