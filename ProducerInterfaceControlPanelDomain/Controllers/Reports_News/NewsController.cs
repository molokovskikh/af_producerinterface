using System.Web.Mvc;
using System.Linq;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class NewsController : MasterBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
           ViewBag.Pager = 1;
           var ListNews10 = cntx_.NotificationToProducers.OrderByDescending(xxx=>xxx.DatePublication).Take(10).ToList();
           return View(ListNews10);
        }

        public ActionResult GetNextList(int Pager)
        {
            ViewBag.Pager = Pager + 1;
            var ListNews10 = cntx_.NotificationToProducers.OrderByDescending(xxx => xxx.DatePublication).Skip(Pager*10).Take(10).ToList();
            return PartialView("GetNextList",ListNews10);
        }

        [HttpGet]
        public ActionResult Create(long Id = 0)
        {
            var NewsModel = new ProducerInterfaceCommon.ContextModels.NotificationToProducers();
            if (Id > 0)
            {
                NewsModel = cntx_.NotificationToProducers.Find(Id);
            }
          
            return View(NewsModel);
        }

        public ActionResult DeleteNews(long Id)
        {
                    
            cntx_.NotificationToProducers.Remove(cntx_.NotificationToProducers.Find(Id));
            cntx_.SaveChanges(CurrentUser, "Удаление новости");

            SuccessMessage("Новость удалена");

            return RedirectToAction("Index");

        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(ProducerInterfaceCommon.ContextModels.NotificationToProducers News)
        {
            if (!ModelState.IsValid)
            {
                return View(News);
            }

            var HistoryNews = new ProducerInterfaceCommon.ContextModels.NewsChange();
            HistoryNews.Account = CurrentUser;
            HistoryNews.NewsNewTema = News.Name;
            HistoryNews.NewsNewDescription = News.Description;
            HistoryNews.DateChange = System.DateTime.Now;

            if (News.Id > 0)
            {
                var OldNews = cntx_.NotificationToProducers.Find(News.Id);

                HistoryNews.DateChange = System.DateTime.Now;  
                HistoryNews.NewsOldTema = OldNews.Name;
                HistoryNews.NewsOldDescription = OldNews.Description;
                HistoryNews.TypeCnhange = (byte)ProducerInterfaceCommon.ContextModels.NewsChanges.NewsChange;
                HistoryNews.NotificationToProducers = OldNews;
                OldNews.DatePublication = System.DateTime.Now;
                OldNews.Description = News.Description;
                OldNews.Name = News.Name;
                cntx_.Entry(OldNews).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges();
                cntx_.NewsChange.Add(HistoryNews);
               // cntx_.Entry(HistoryNews).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();
                SuccessMessage("Изменения успешно сохранены");
            }
            else {
                HistoryNews.TypeCnhange = (byte)ProducerInterfaceCommon.ContextModels.NewsChanges.NewsAdd;             
                News.DatePublication = System.DateTime.Now;
                cntx_.Entry(News).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();

                HistoryNews.NotificationToProducers = News;            
                cntx_.NewsChange.Add(HistoryNews);


                cntx_.SaveChanges();

                SuccessMessage("Новость успешно добавлена");
            }
            
          

            return RedirectToAction("List");
        }

    }
}
