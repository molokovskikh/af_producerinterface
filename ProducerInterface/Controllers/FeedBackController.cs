using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Global;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;

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

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.TypeMessage = System.Enum.GetValues(typeof(FeedBackType)).Cast<FeedBackType>();        
            return View();
        }

        [HttpGet]
        public ActionResult Index_Type(sbyte Id)
        {
            ViewBag.FBT = (FeedBackType)Id;

            ViewBag.TypeMessage = System.Enum.GetValues(typeof(FeedBackType)).Cast<FeedBackType>();
            var FeedModel = new FeedBack();

            if (Id == (sbyte)FeedBackType.AddNewAppointment)
            {
                FeedModel.Description = "Просьба добавить мою должность: ";
            }

         
            return View("Index", FeedModel);
        }

        [HttpPost]
        public ActionResult Index(FeedBack FB)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TypeMessage = System.Enum.GetValues(typeof(FeedBackType)).Cast<FeedBackType>();
                ViewBag.FBT = (FeedBackType)FB.FeedType;
                return View(FB);
            }
            
            var FB_Save = new ProducerInterfaceCommon.ContextModels.AccountFeedBack();
            FB_Save.Description = FB.Description;
            FB_Save.AccountId = CurrentUser.Id;
            FB_Save.Type = FB.FeedType;
            FB_Save.Contacts = FB.Contact;
            FB_Save.DateAdd = System.DateTime.Now;
            FB_Save.UrlString = "Обратная взязь в ЛК";
            cntx_.AccountFeedBack.Add(FB_Save);
            cntx_.SaveChanges();

            SuccessMessage("Выша заявка принята к исполнению");

            return RedirectToAction("Index", "Profile");
        }



        public ActionResult GetView()
        {          
            var ModelView = new FeedBack();
            return PartialView("FeedBack", ModelView);
        }
    }
}
