using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
    public class FeedBackFunction
    {
        private producerinterface_Entities cntx_;
        private Account CurrentUser;

        public FeedBackFunction(Account currentUser)
        {
            cntx_ = new producerinterface_Entities();
            CurrentUser = currentUser;
        }

        public FeedBackFilterView GetModelView(FeedBackFilter FeedFilter = null)
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
          
            var Ret = new FeedBackFilterView();
            if (FeedFilter == null)
            {
                return GetDefaultFeedBackFilterView(h);
            }
            else
            {
                return GetFeedBackFilterView(h, FeedFilter);
            }          
        }

        public FeedBackFilterView GetFeedBackFilterView(Heap.NamesHelper h, FeedBackFilter FeedFilter)
        {
            var ModelView = new List<FeedBackViewModel>(); /* лист обращений */

            int MaxListCount = 0;    /*  количество сторок по данному фильтру необходимо для посторение Пагинатора */

            ModelView = GetListFeedBackModel(ref MaxListCount,FeedFilter); /* заполняем лист обращений по фильтру */
            if (ModelView == null || ModelView.Count() == 0)
            {
                return new FeedBackFilterView();  /* если обращений по данному фильтру не найдено, возвращаем незаполненую модель для передачи на клиент сообщения об неуспешности фильтрации */
            }

            var ReturnModel = new FeedBackFilterView();  /* Модель отправляемая клиенту */

            SetViewFilter(ref ReturnModel, FeedFilter);  /* отображение по каким столбцам фильтруется таблица */

            ReturnModel.FeedBackList = ModelView; /* FeedBackList - список обращений */

            ReturnModel.PaginatorLinks = GetPaginator(MaxListCount, FeedFilter.PagerIndex); /* заполняем пагинатор */

            ReturnModel.PageIndex = FeedFilter.PagerIndex; /* текущий индех пагинатора */

            ReturnModel.FeedBackFilter = FeedFilter;
            
            int MaxPager = (int)Math.Ceiling((double)(Convert.ToInt32(GetPageCountList().Where(x=>x.Id == FeedFilter.Pager).First().Name) / 50));
                 
            return ReturnModel;
        }


        public FeedBackFilterView GetDefaultFeedBackFilterView(Heap.NamesHelper h)
        {
            var ModelView = new List<FeedBackViewModel>();
            var cntx_model = cntx_.AccountFeedBack.OrderByDescending(x => x.DateAdd).Take(50).ToList();

            AccountFeedBackToFeedBackViewModelConverter(ref ModelView, cntx_model);
              
            var ReturnModel = new FeedBackFilterView();

            SetViewFilter(ref ReturnModel); // устанавливаем CSS параметр Display: ???

            ReturnModel.FeedBackList = ModelView;
                  
            int MaxPager = (int) Math.Ceiling((double)(ModelView.Count() / 50));

            ReturnModel.PaginatorLinks = GetPaginator(MaxPager, 1);
            ReturnModel.PageIndex = 1;
            ReturnModel.MaxPageCount = MaxPager;
            ReturnModel.FeedBackFilter = new FeedBackFilter
            {
                DateBegin = DateTime.Now.AddYears(-1).ToString("dd.MM.yyyy"),
                DateEnd = DateTime.Now.AddDays(1).ToString("dd.MM.yyyy"),
                LoginId = 0,
                ProducerId = 0,
                PagerIndex = 1,
                Pager = 1
            };

            ReturnModel.PageCount = GetPageCountList();
            ReturnModel.ProducerList = GetProducerList(h);
            ReturnModel.AccountList = GetAccountList(h);

            return ReturnModel;
        }

        private void SetViewFilter(ref FeedBackFilterView ModelView, FeedBackFilter FeedFilter = null)
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
                if (FeedFilter.LoginId != 0)
                { ModelView.SortAccountName = "block"; }            


                if (FeedFilter.ProducerId != 0)
                { ModelView.SortProducerName = "block"; }     
              
            }
        }

        private List<FeedBackViewModel> GetListFeedBackModel(ref int MaxListCount,FeedBackFilter FeedFilter)
        {
            List<FeedBackViewModel> ret = new List<FeedBackViewModel>();       
            var AccountFeedBackList = GetFeedBackList(ref MaxListCount, FeedFilter);

            if (AccountFeedBackList == null || AccountFeedBackList.Count() == 0)
            {
                return ret;
            }

            AccountFeedBackToFeedBackViewModelConverter(ref ret, AccountFeedBackList);

            return ret;
        }

        private void AccountFeedBackToFeedBackViewModelConverter(ref List<FeedBackViewModel> FB_ViewModel, List<AccountFeedBack> Account_FB_List)
        {
            FB_ViewModel = Account_FB_List.Select(x => new FeedBackViewModel
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

        private List<ViewAccount> GetAccountList(Heap.NamesHelper h)
        {
            var AccountList = new List<ViewAccount>() { new ViewAccount { Id = "0", Name = "Выберите пользователя" } };
            AccountList.AddRange(cntx_.Account.Where(x => x.TypeUser == (sbyte)TypeUsers.ProducerUser && x.Enabled == 1).ToList().Select(x => new ViewAccount { Name = x.Login + " " + x.Name, Id = x.Id.ToString() }).ToList());

            return AccountList;
        }

        private List<PageCount> GetPageCountList()
        {
            return new List<PageCount>()
            {
                new PageCount { Id = 0, Name = "20" },
                new PageCount { Id = 1, Name = "50" },
                new PageCount { Id = 2, Name = "100" },
                new PageCount { Id = 3, Name = "1" }
            };
        }

        private List<ViewProducer> GetProducerList(ProducerInterfaceCommon.Heap.NamesHelper h)
        {
            var ProducerList = new List<ViewProducer>() { new ViewProducer
            {
                 Name = "Выберите производителя", Id = "0"
            } };

            ProducerList.AddRange(h.RegisterListProducer().ToList().Select(x => new ViewProducer { Id = x.Value, Name = x.Text }).ToList());

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
                if (feedFilter.LoginId != 0)
                {
                    ret = ret.Where(xx => xx.Account != null).Where(xx => xx.AccountId == feedFilter.LoginId).ToList();
                }
                if (ret == null || ret.Count() == 0)
                {
                    return ret;
                }

                var PageCountList = GetPageCountList();

                var OnePageListLeght = Convert.ToInt32(PageCountList.Where(x => x.Id == feedFilter.Pager).First().Name);

                ret = ret.OrderByDescending(x => x.Id).ToList();

                if (ret.Count() <= OnePageListLeght)
                {
                    return ret;
                }
                if (ret.Count() > OnePageListLeght)
                {
                    MaxCount = ret.Count();                 
                    ret = ret.ToList().Skip((OnePageListLeght)* feedFilter.PagerIndex).Take(OnePageListLeght).ToList();                  
                }

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
