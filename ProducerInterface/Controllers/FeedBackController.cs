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
                if (string.IsNullOrEmpty(Model_View.PhoneNum) && string.IsNullOrEmpty(Model_View.Email))
                {
                    ViewBag.ErrorMessageModal = "Укажите контакты для связи. Email или номер телефона";
                    return PartialView("FeedBack", Model_View);
                }

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
