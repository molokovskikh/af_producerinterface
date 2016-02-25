using System.Web.Mvc;

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


    }
}
