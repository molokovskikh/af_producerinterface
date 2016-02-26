using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Global;

namespace ProducerInterface.Controllers
{
    public class FeedBackController : MasterBaseController
    {      
        public ActionResult Index(string PageUrl, string UdobstvaSvyazi, string Id = null)
        {
            if (Id == null)
            {
                return Content("Заполните текст сообщения");
            }

            var FeedBack = new ProducerInterfaceCommon.ContextModels.AccountFeedBack();

            if (CurrentUser != null)
            {
                FeedBack.AccountId = CurrentUser.Id;
            }

            FeedBack.DateAdd = System.DateTime.Now;
            FeedBack.Description = "" + PageUrl + System.Environment.NewLine + ">";
            FeedBack.Description += "" + UdobstvaSvyazi + System.Environment.NewLine + ">";
            FeedBack.Description += "" + System.Environment.NewLine + Id;

            cntx_.Entry(FeedBack).State = System.Data.Entity.EntityState.Added;
            cntx_.SaveChanges();

            return Content("<p> Ваша заявка принята</p><button type = 'button' class='btn btn-primary' data-dismiss='modal' onclick='serverMessageCleanForm()'>Закрыть</button>");
        }

        [HttpPost]
        public ActionResult SaveFeedBack(FeedBack Model_View)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("FeedBack", Model_View);
            }
            else
            {
                // сохраняем

                return PartialView("Success");
            }

        }

        [HttpGet]
        public ActionResult GetView()
        {
            var ModelView = new FeedBack();
            return PartialView("FeedBack", ModelView);
        }
    }
}
