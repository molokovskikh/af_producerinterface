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
           var ListNews10 = cntx_.NotificationToProducers.Take(10).ToList();
           return View(ListNews10);
        }

        public ActionResult GetNextList(int Pager)
        {
            ViewBag.Pager = Pager + 1;
            var ListNews10 = cntx_.NotificationToProducers.OrderBy(xxx=>xxx.Id).Take(10).ToList();
            return PartialView("GetNextList",ListNews10);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var NewsModel = new ProducerInterfaceCommon.ContextModels.NotificationToProducers();
            return View(NewsModel);
        }
        [HttpPost]
        public ActionResult Create(ProducerInterfaceCommon.ContextModels.NotificationToProducers News)
        {
            if (!ModelState.IsValid)
            {
                return View(News);
            }

            News.DatePublication = System.DateTime.Now;
            cntx_.Entry(News).State = System.Data.Entity.EntityState.Added;
            cntx_.SaveChanges();

            SuccessMessage("Новость успешно добавлена");
            return RedirectToAction("List");
        }

    }
}
