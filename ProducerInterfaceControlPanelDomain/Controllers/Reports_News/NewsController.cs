using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using System.ComponentModel.DataAnnotations;
using System;

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
            ViewBag.Title = "Новости";
            ViewBag.Pager = 1;
            var ListNews10 = cntx_.NotificationToProducers.Where(xxx => xxx.Enabled == true).OrderByDescending(xxx => xxx.DatePublication).Take(10).ToList();
            return View(ListNews10);
        }

        [ValidateInput(false)]
        public ActionResult Preview(string Name, string Description)
        {
            var News = new ProducerInterfaceCommon.ContextModels.NotificationToProducers();
            News.DatePublication = System.DateTime.Now;
            News.Name = Name;
            News.Description = Description;
            return View(News);
        }

        public ActionResult GetNextList(int Pager)
        {
            ViewBag.Pager = Pager + 1;
            var ListNews10 = cntx_.NotificationToProducers.Where(xxx => xxx.Enabled == true).OrderByDescending(xxx => xxx.DatePublication).Skip(Pager * 10).Take(10).ToList();
            return PartialView("GetNextList", ListNews10);
        }

        public ActionResult Archive()
        {
            ViewBag.Title = "Архив Новостей";
            var ListNews10 = cntx_.NotificationToProducers.Where(xxx => xxx.Enabled == false).OrderByDescending(xxx => xxx.DatePublication).ToList();
            return View("List", ListNews10);
        }

        public ActionResult DeleteNews(long Id)
        {
            // отправляем новость в архив
            var NewsRemove = cntx_.NotificationToProducers.Find(Id);
            NewsRemove.Enabled = false;
            cntx_.Entry(NewsRemove).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            // добавляем историю изменений
            NewsHistoryAdd(NewsRemove.Id, null, null, ProducerInterfaceCommon.ContextModels.NewsChanges.NewsArchive);

            SuccessMessage("Новость отправлена в архив");
            return RedirectToAction("List");
        }

        public ActionResult PastRetrieve(long Id)
        {
            var News = cntx_.NotificationToProducers.Find(Id);

            var NewsHistoryList = cntx_.NewsChange.Where(xxx => xxx.IdNews == Id).ToList();

            cntx_.NewsChange.RemoveRange(NewsHistoryList);
            cntx_.SaveChanges();

            cntx_.Entry(News).State = System.Data.Entity.EntityState.Deleted;
            cntx_.SaveChanges(CurrentUser, "Удаление новости");

            SuccessMessage("Новость удалена");
            return RedirectToAction("Archive");

        }


        [HttpGet]
        public ActionResult Create(long Id = 0)
        {
            ViewBag.FullUrlStringFile = GetWebConfigParameters("ImageFullUrlString");

#if DEBUG
            {
                ViewBag.FullUrlStringFile = this.Request.Url.Segments[0] + @"mediafiles/";
            }
#endif


            var NewsModel = new ProducerInterfaceCommon.ContextModels.NotificationToProducers();
            if (Id > 0)
            {
                NewsModel = cntx_.NotificationToProducers.Find(Id);
            }
            return View(NewsModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(ProducerInterfaceCommon.ContextModels.NotificationToProducers News)
        {

     

            if (!ModelState.IsValid)
            {
                return View(News);
            }

            if (News.Id > 0)
            {
                var OldNews = cntx_.NotificationToProducers.Find(News.Id);

                // добавляем в историю изменения
                NewsHistoryAdd(News.Id, OldNews, News, ProducerInterfaceCommon.ContextModels.NewsChanges.NewsChange);

                // изменияем новость
                OldNews.DatePublication = System.DateTime.Now;
                OldNews.Description = News.Description;
                OldNews.Name = News.Name;
                OldNews.Enabled = true;

                cntx_.Entry(OldNews).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges();

                SuccessMessage("Изменения успешно сохранены");
            }
            else {

                // добавляем новость
                News.DatePublication = System.DateTime.Now;
                News.Enabled = true;
                cntx_.Entry(News).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();

                // пишем в историю
                NewsHistoryAdd(News.Id, null, News, ProducerInterfaceCommon.ContextModels.NewsChanges.NewsAdd);

                SuccessMessage("Новость успешно добавлена");
            }

            return RedirectToAction("List");
        }

        public ActionResult History(long Id =0)
        {
            if (Id == 0)
            {
                ErrorMessage("Неверный номер истории изменений");
                return RedirectToAction("List");
            }
            var Model_View = cntx_.NewsChange.Find(Id);

            if (Model_View == null)
            {
                ErrorMessage("Неверный номер истории изменений");
                return RedirectToAction("List");
            }
            
            return View(Model_View);
        }
        

        private void NewsHistoryAdd(long ID_NEWS, ProducerInterfaceCommon.ContextModels.NotificationToProducers OldNews, ProducerInterfaceCommon.ContextModels.NotificationToProducers NewNews, ProducerInterfaceCommon.ContextModels.NewsChanges TypeChanges)
        {

            var NewsHistory = new ProducerInterfaceCommon.ContextModels.NewsChange();

            NewsHistory.IdNews = ID_NEWS;
            NewsHistory.IdAccount = CurrentUser.Id;
            NewsHistory.DateChange = System.DateTime.Now;
            NewsHistory.TypeCnhange = (byte)TypeChanges;
            NewsHistory.IP = CurrentUser.IP;

            if (TypeChanges == ProducerInterfaceCommon.ContextModels.NewsChanges.NewsAdd)
            {
                NewsHistory.NewsNewDescription = NewNews.Description;
                NewsHistory.NewsNewTema = NewNews.Name;
            }

            if (TypeChanges == ProducerInterfaceCommon.ContextModels.NewsChanges.NewsChange)
            {
                NewsHistory.NewsNewDescription = NewNews.Description;
                NewsHistory.NewsNewTema = NewNews.Name;

                NewsHistory.NewsOldDescription = OldNews.Description;
                NewsHistory.NewsOldTema = OldNews.Name;
            }

            if (TypeChanges == ProducerInterfaceCommon.ContextModels.NewsChanges.NewsArchive)
            {
                NewsHistory.NewsNewTema = cntx_.NotificationToProducers.Find(ID_NEWS).Name;
                NewsHistory.NewsNewDescription = cntx_.NotificationToProducers.Find(ID_NEWS).Description;
                NewsHistory.NewsOldTema = cntx_.NotificationToProducers.Find(ID_NEWS).Name;
                NewsHistory.NewsOldDescription = cntx_.NotificationToProducers.Find(ID_NEWS).Description;
            }
            
            cntx_.Entry(NewsHistory).State = System.Data.Entity.EntityState.Added;
            cntx_.SaveChanges();
        }
    }
}
