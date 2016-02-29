using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Global;

namespace ProducerInterface.Controllers
{
    public class FeedBackController : MasterBaseController
    {      

        [HttpPost]
        public ActionResult SaveFeedBack(FeedBack Model_View)
        {
            if (!ModelState.IsValid)
            {              
                return PartialView("FeedBack", Model_View);
            }
            else
            {
                if (string.IsNullOrEmpty(Model_View.PhoneNum) && string.IsNullOrEmpty(Model_View.Email) && CurrentUser == null)
                {
                    ViewBag.ErrorMessageModal = "Укажите контакты для связи. Email или номер телефона";
                    return PartialView("FeedBack", Model_View);
                }

                var FeedBackAdd = new ProducerInterfaceCommon.ContextModels.AccountFeedBack();

                if (CurrentUser != null)
                {
                    FeedBackAdd.Contacts = Model_View.Contact;
                    FeedBackAdd.AccountId = CurrentUser.Id;
                }
                else
                {
                    FeedBackAdd.Contacts = Model_View.ContactNotAuth;
                }

                FeedBackAdd.Description = Model_View.Description;
                FeedBackAdd.UrlString = Model_View.Url;
                FeedBackAdd.DateAdd = System.DateTime.Now;
                FeedBackAdd.Type = Model_View.FeedType;
                cntx_.AccountFeedBack.Add(FeedBackAdd);
                cntx_.SaveChanges();               

                return PartialView("Success");
            }

        }
              
        public ActionResult GetView()
        {
            var ModelView = new FeedBack();
            return PartialView("FeedBack", ModelView);
        }
    }
}
