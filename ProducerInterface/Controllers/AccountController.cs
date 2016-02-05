using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class AccountController : MasterBaseController
    {
        // GET: Account
        public ActionResult Index()
        {
            RegistrationAccountValidation ModelView = new RegistrationAccountValidation();
            ViewBag.ProducerList = cntx_.producernames.Select(xxx => new OptionElement { Text = xxx.ProducerName, Value=xxx.ProducerId.ToString() }).ToList();
            return View(ModelView);
        }

        [HttpGet]
        public ActionResult Registration(RegistrationAccountValidation One)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", One);
            }
                        
            var CompanyExsist = cntx_.AccountCompany.Any(xxx => xxx.ProducerId == One.Producers);

            RegistrerValidation ModelView = new RegistrerValidation();
            ModelView.Producers = One.Producers;
            ModelView.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == One.Producers).First().ProducerName;

            if (CompanyExsist)
            {
                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == One.Producers).First().CompanyDomainName.Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                return View("DomainRegistration",ModelView);
            }        
            
        
         
            return View(ModelView);
        }

        [HttpPost]
        public ActionResult DomainRegistration()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Registration(RegistrerValidation ModelAccount)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelAccount);
            }

            var Company = new AccountCompany();
            var ExsistCompany = cntx_.AccountCompany.Any(xxx => xxx.ProducerId == ModelAccount.Producers);

            var UserExsist = cntx_.Account.Any(xxx => xxx.Login == ModelAccount.login);

            if (UserExsist)
            {
                ErrorMessage("Данный емаил уже зарегистрирован в нашей базе, попробуйте восстановить пароль");
                return View(ModelAccount);
            }
            
            if (ExsistCompany)
            {
                Company = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == ModelAccount.Producers).First();             
            }
            else
            {
                Company.ProducerId = ModelAccount.Producers;
                Company.Name = ModelAccount.ProducerName;
                cntx_.Entry(Company).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();
                string DomainName = ModelAccount.login.ToString().Split(new Char[] { '@' }, StringSplitOptions.RemoveEmptyEntries)[1].ToLower();
                var X = new CompanyDomainName();
                X.Name = DomainName;
                X.CompanyId = Company.Id;
                cntx_.Entry(X).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();
                Company.CompanyDomainName.Add(X);
                cntx_.SaveChanges();

            }

            cntx_.SaveChanges();

            //Account New_User = new Account();
            //New_User.Login = NewAccount.login;
            //New_User.Name = NewAccount.Name;
            //New_User.Enabled = 0;
            //New_User.Appointment = NewAccount.Appointment;
            //string Password = GetRandomPassword();
            //New_User.Password = Md5HashHelper.GetHash(Password);
            //New_User.CompanyId = NewAccount.Producers;
            //New_User.UserType = TypeUsers.ProducerUser;
            //New_User.PasswordUpdated = SystemTime.GetDefaultDate();
            //cntx_.Entry(New_User).State = System.Data.Entity.EntityState.Added;



            var NewAccount = new Account();

            NewAccount.Enabled = 0;
            NewAccount.CompanyId = Company.Id;
            NewAccount.Login = ModelAccount.login;
            NewAccount.Name = ModelAccount.Name;
            string Password = GetRandomPassword();
            NewAccount.Password = Md5HashHelper.GetHash(Password);
            NewAccount.PasswordUpdated = SystemTime.GetDefaultDate();
            NewAccount.Phone = ModelAccount.PhoneNumber;
            NewAccount.Appointment = ModelAccount.Appointment;
            NewAccount.UserType = TypeUsers.ProducerUser;
            cntx_.Entry(NewAccount).State = System.Data.Entity.EntityState.Added;

            cntx_.SaveChanges();

            string AdminGroupName = GetWebConfigParameters("AdminGroupName");

            NewAccount.AccountGroup.Add(cntx_.AccountGroup.Where(xxx => xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser).First());
            cntx_.SaveChanges();

            ProducerInterfaceCommon.Heap.EmailSender.SendRegistrationMessage(cntx_, NewAccount.Id, Password, NewAccount.IP);

            return RedirectToAction("Index", "Home");
        }


        public ActionResult CustomRegistration()
        {
            return View();
        }

    }
}