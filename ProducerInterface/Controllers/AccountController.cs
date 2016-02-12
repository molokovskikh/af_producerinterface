using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class AccountController : AccountAuthController
    {
        // GET: Account
        public ActionResult Index()
        {
            var ModelView = new RegistrationAccountValidation();
            var ProducerList = new List<OptionElement>() {new OptionElement() {Text = "", Value = ""}};
            ProducerList.AddRange(
                cntx_.producernames.Select(
                    xxx => new OptionElement {Text = xxx.ProducerName, Value = xxx.ProducerId.ToString()}).ToList());

            ViewBag.ProducerList = ProducerList;
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
            ViewBag.AppointmentList =
                 cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                     .ToList()
                     .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                     .ToList();
            if (CompanyExsist)
            {
                RegisterDomainValidation ModelView2 = new RegisterDomainValidation();
                ModelView2.Producers = One.Producers;
                ModelView2.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == One.Producers).First().ProducerName;

                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == One.Producers).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();
                return View("DomainRegistration", ModelView2);
            }

            RegistrerValidation ModelView = new RegistrerValidation();
            ModelView.Producers = One.Producers;
            ModelView.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == One.Producers).First().ProducerName;

            return View(ModelView);
        }
        [HttpPost]
        public ActionResult Registration(RegistrerValidation ModelAccount)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AppointmentList = cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                   .ToList()
                   .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                   .ToList();
                return View(ModelAccount);
            }

            var Company = new AccountCompany();
            var ExsistCompany = cntx_.AccountCompany.Any(xxx => xxx.ProducerId == ModelAccount.Producers);

            var UserExsist = cntx_.Account.Any(xxx => xxx.Login == ModelAccount.login);

            if (UserExsist)
            {
                ErrorMessage("Данный емаил уже зарегистрирован в нашей базе, попробуйте восстановить пароль");
                ViewBag.AppointmentList = cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                   .ToList()
                   .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                   .ToList();
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

            var NewAccount = new Account();

            NewAccount.Enabled = 0;
            NewAccount.CompanyId = Company.Id;
            NewAccount.Login = ModelAccount.login;
            NewAccount.Name = ModelAccount.Name;
            string Password = GetRandomPassword();
            NewAccount.Password = Md5HashHelper.GetHash(Password);
            NewAccount.PasswordUpdated = SystemTime.GetDefaultDate();
            NewAccount.Phone = ModelAccount.PhoneNumber;
            NewAccount.AppointmentId = ModelAccount.AppointmentId;
            NewAccount.Appointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == ModelAccount.AppointmentId).First().Name; // ModelAccount.Appointment;
            NewAccount.UserType = TypeUsers.ProducerUser;
            cntx_.Entry(NewAccount).State = System.Data.Entity.EntityState.Added;

            cntx_.SaveChanges();

            string AdminGroupName = GetWebConfigParameters("AdminGroupName");

            NewAccount.AccountGroup.Add(cntx_.AccountGroup.Where(xxx => xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser).First());
            cntx_.SaveChanges();

            ProducerInterfaceCommon.Heap.EmailSender.SendRegistrationMessage(cntx_, NewAccount.Id, Password, NewAccount.IP);

            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public ActionResult DomainRegistration(RegisterDomainValidation modelAccount)
        {
         //   var ModelView = new RegisterDomainValidation();
            if (!ModelState.IsValid)
            {
                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == modelAccount.Producers).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();

                ViewBag.AppointmentList = cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                     .ToList()
                     .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                     .ToList();
                return View(modelAccount);
            }

            var EmailAdress = modelAccount.Mailname + "@" + cntx_.CompanyDomainName.Where(xxx => xxx.Id == modelAccount.EmailDomain).First().Name;

            bool UserExsist = cntx_.Account.Any(xxx => xxx.Login == EmailAdress);

            if (UserExsist)
            {
                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == modelAccount.Producers).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.AppointmentList = cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                     .ToList()
                     .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                     .ToList();

                ErrorMessage("Данный eMail уже зарегистрирован в нашей базе, попробуйте восстановить пароль");
                return View(modelAccount);
            }


            var NewAccount = new Account();

            NewAccount.Login = modelAccount.Mailname + "@" + cntx_.CompanyDomainName.Where(xxx=>xxx.Id == modelAccount.EmailDomain).First().Name;
            NewAccount.Name = modelAccount.Name;
            NewAccount.UserType = TypeUsers.ProducerUser;
            NewAccount.PasswordUpdated = DateTime.MinValue;
            NewAccount.Enabled = 0;
            NewAccount.Appointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == modelAccount.AppointmentId).First().Name;
            NewAccount.CompanyId = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == modelAccount.Producers).First().Id;

            string Password = GetRandomPassword();

            NewAccount.Password = Md5HashHelper.GetHash(Password);

            // Password send To User eMail
        //    NewAccount.AccountCompany = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == modelAccount.Producers).First();
            NewAccount.Phone = modelAccount.PhoneNumber;

            cntx_.Entry(NewAccount).State = EntityState.Added;
            cntx_.SaveChanges();

       
      //      cntx_.Entry(NewAccount).State = EntityState.Modified;
      //      cntx_.SaveChanges();

            // добавляем в группу все пользователи

            var Group_NewAccount = cntx_.AccountGroup.Where(xxx => xxx.Name == "все" && xxx.TypeGroup == NewAccount.TypeUser).FirstOrDefault();

            if (Group_NewAccount == null)
            {
                Group_NewAccount = new AccountGroup() { Name = "все", Description = "вновь зарегистрированные пользователи" };
                Group_NewAccount.Enabled = true;
                cntx_.Entry(Group_NewAccount).State = EntityState.Added;
                cntx_.SaveChanges();

                List<AccountPermission> LST_Permission = cntx_.AccountPermission.ToList().Where(xxx => xxx.Enabled == true && xxx.TypePermission == NewAccount.TypeUser).ToList();

                for (int i = 0; i < LST_Permission.Count(); i++)
                {
                    Group_NewAccount.AccountPermission.Add(LST_Permission[i]);    
                }
                cntx_.Entry(Group_NewAccount).State = EntityState.Modified;
                cntx_.SaveChanges();

            }

            Group_NewAccount.Account.Add(NewAccount);
            cntx_.Entry(Group_NewAccount).State = EntityState.Modified;

            NewAccount.AccountAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == modelAccount.AppointmentId).First();
         
            cntx_.Entry(NewAccount).State = EntityState.Modified;
            
            cntx_.SaveChanges();

            ProducerInterfaceCommon.Heap.EmailSender.SendRegistrationMessage(cntx_, NewAccount.Id, Password, HttpContext.Request.UserHostAddress.ToString());

            return RedirectToAction("Index", "Home");

        }
           
        [HttpPost]
        public ActionResult DolznostAddNew(string NewNameAppointment)
        {
            var NewAppExsist = cntx_.AccountAppointment.Any(xxx => xxx.Name == NewNameAppointment);
            var NewAppointment_ = new AccountAppointment();

            if (NewAppExsist)
            {
                NewAppointment_ = cntx_.AccountAppointment.Where(xxx => xxx.Name == NewNameAppointment).First();
            }
            else
            {
                NewAppointment_.Name = NewNameAppointment;
                NewAppointment_.GlobalEnabled = 0;
                cntx_.Entry(NewAppointment_).State = EntityState.Added;
                cntx_.SaveChanges();
            }

            return Content(NewAppointment_.Id.ToString() + ";" + NewAppointment_.Name);            
        }
            
        [HttpGet]
        public ActionResult CustomRegistration()
        {
            ViewBag.AppointmentList = cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                   .ToList()
                   .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                   .ToList();

            var ViewModel = new RegisterCustomValidation();
            return View(ViewModel);
        }

        [HttpPost] 
        public ActionResult CustomRegistration(RegisterCustomValidation NewAccount)
        {
            if (!ModelState.IsValid)
            {
                return View(NewAccount);
            }

            // добавляем в БД, без каких либо прав
            // отсылаем письмо об регистрации одиночного пользователя   // не реализовано
            
            var CompanyCreate = new AccountCompany();

            CompanyCreate.Name = NewAccount.CompanyName; // Комментарий пользователя, далее название компании
            cntx_.Entry(CompanyCreate).State = EntityState.Added;
            cntx_.SaveChanges();

            var AccountCreate = new Account();

            AccountCreate.Login = NewAccount.login;
            AccountCreate.Enabled = 0;
            AccountCreate.TypeUser = 0;
            AccountCreate.Appointment = NewAccount.Appointment;
            AccountCreate.Phone = NewAccount.PhoneNumber;
            AccountCreate.PasswordUpdated = DateTime.Now;
            cntx_.Entry(AccountCreate).State = EntityState.Added;
            cntx_.SaveChanges();

            AccountCreate.AccountCompany = CompanyCreate;
            cntx_.Entry(AccountCreate).State = EntityState.Modified;
            cntx_.SaveChanges();
            
            SuccessMessage("Ваша заявка принята.   eMail пока не отправляется, в панеле управления можно посмотреть список заявок на регистрацию");
            return View();

        }          

    }
}