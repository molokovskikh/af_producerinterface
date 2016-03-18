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
        public ActionResult Index_Type(sbyte Id, long IdProducer = 0)
        {
            ViewBag.FBT = (FeedBackTypePrivate)Id;

            ViewBag.TypeMessage = System.Enum.GetValues(typeof(FeedBackTypePrivate)).Cast<FeedBackTypePrivate>();
            var FeedModel = new FeedBack();

            if (Id == (sbyte)FeedBackTypePrivate.AddNewAppointment)
            {
                FeedModel.Description = "Просьба добавить мою должность: ";

            }

            if (Id == (sbyte)FeedBackTypePrivate.AddNewDomainName)
            {             
                var ProducerName = cntx_.producernames.Where(x => x.ProducerId == IdProducer).First().ProducerName;
                ViewBag.ProducerName = ProducerName;
                FeedModel.Description = "Я являясь сотрудником компании " + ProducerName + ", не могу зарегистрироваться в связи с отсутствием домена моего почтового ящика, прошу добавить возможность регистрации с моим eMail";
                return View("FeedBackNewDomain", FeedModel);
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

            if (CurrentUser == null && string.IsNullOrEmpty(FB.Email) && FB.FeedType == (sbyte)FeedBackTypePrivate.AddNewDomainName)
            {
                ViewBag.FBT = (FeedBackTypePrivate)FB.FeedType;
                ViewBag.TypeMessage = System.Enum.GetValues(typeof(FeedBackTypePrivate)).Cast<FeedBackTypePrivate>();

                ViewBag.ErrorMessageModal = "поле Email является обязательным для заполнения";

                return View("Index", FB);              
            }
                    
            var FB_Save = new ProducerInterfaceCommon.ContextModels.AccountFeedBack();

            if (FB.FeedType == (sbyte)FeedBackTypePrivate.AddNewAppointment)
            {
                FB_Save.UrlString = "Обратная взязь в ЛК";
            }
            if (FB.FeedType == (sbyte)FeedBackTypePrivate.AddNewDomainName)
            {
                FB_Save.UrlString = "Регистрация нового пользователя для зарегистированного производителя";
            }
            if (string.IsNullOrEmpty(FB_Save.UrlString))
            {
                FB_Save.UrlString = "~/FeedBack/Index_Type/";
            }

            FB_Save.Description = FB.Description;

            if (CurrentUser != null)
            {
                FB_Save.AccountId = CurrentUser.Id;
            }
      
            FB_Save.Type = FB.FeedType;
            FB_Save.Contacts = FB.Contact;
            FB_Save.DateAdd = System.DateTime.Now;         
            cntx_.AccountFeedBack.Add(FB_Save);
            cntx_.SaveChanges();

            SuccessMessage("Выша заявка принята к исполнению");

            return RedirectToAction("Index", "Profile");
        }

        public ActionResult FeedBakcAddNewDomain(string FIO, string Email, string PhoneNum, string CompanyNames)
        {

            AccountFeedBack AFB = new AccountFeedBack();

            AFB.Contacts = PhoneNum + "," + Email;
            AFB.Type = (sbyte)FeedBackTypePrivate.AddNewDomainName;
            AFB.Status =(sbyte) FeedBackStatus.FeedBack_New;
            AFB.UrlString = "Заявка подана при невозможности зарегистрироватся с другим доменом для производителя: " + CompanyNames;
            AFB.Description = "Я, " + FIO + ", являясь сотрудником компании " + CompanyNames + ", использую в своей деятельности E-mail: " + Email + " Однако система не позволяет мне зарегистрироваться с этим E-mail. Прошу решить возникшую  проблему. Телефон для связи: " + PhoneNum;
            AFB.DateAdd = System.DateTime.Now;
            
            cntx_.AccountFeedBack.Add(AFB);
            cntx_.SaveChanges();
            
            SuccessMessage("Выша заявка принята к исполнению");
            return RedirectToAction("index", "home");
        }

        public ActionResult GetView()
        {          
            var ModelView = new FeedBack();
            return PartialView("FeedBack", ModelView);
        }
    }
}
