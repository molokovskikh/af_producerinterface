using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Registration;

namespace ProducerInterface.Controllers
{
    public class AccountController : AccountAuthController
    {
        /* заполняет ViewBag.AppointmentList списком должностей (если передан ID производителя, добавляются должности данного производителя) */
        private void ViewBagAppointmentList(long ProducerId = 0)
        {
            var ListAppointment = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
            ListAppointment.AddRange(cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1 && xx.IdAccount == null)
                 .ToList()
                 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                 .ToList());

            if (ProducerId != 0)
            {
                var CompanyCustomAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Account1.AccountCompany.ProducerId == ProducerId).ToList().Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ListAppointment.AddRange(CompanyCustomAppointment);              
            }
            ViewBag.AppointmentList = ListAppointment;
        }

        /* заполняет ViewBag.ProducerList списоком производителей */
        private void ViewBagProducerList()
        {
            var ProducerList = new List<OptionElement>() { new OptionElement() { Text = "", Value = "" } };
            ProducerList.AddRange(
                cntx_.producernames.Select(
                    xxx => new OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList());

            ViewBag.ProducerList = ProducerList;
        }

        // GET: Account
        public ActionResult Index()
        {
            var ViewModel = new RegProducerViewModel();

            ViewBagProducerList();

            return View(ViewModel);
        }

        [HttpGet]
        public ActionResult Registration(RegProducerViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBagProducerList();
                return View("Index", ViewModel);
            }
                                    
            var CompanyExsist = cntx_.AccountCompany.Any(xxx => xxx.ProducerId == ViewModel.ProducerId);
               
            if (CompanyExsist)
            {
                /* если ото данного производитля регистрировались, возвращаем форму для регистрации пользователя с моделью RegDomainViewModel */
                ViewBagAppointmentList();

                var ModelDomainView = new RegDomainViewModel();
                ModelDomainView.Producers = ViewModel.ProducerId;
                ModelDomainView.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == ViewModel.ProducerId).First().ProducerName;                
                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == ViewModel.ProducerId).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();
                return View("DomainRegistration", ModelDomainView);
            }

            ViewBagAppointmentList();
        
            RegViewModel ModelView = new RegViewModel();
            ModelView.ProducerId = ViewModel.ProducerId;
            ModelView.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == ViewModel.ProducerId).First().ProducerName;

            return View(ModelView);
            
            
                        
        }
        [HttpPost]
        public ActionResult Registration(RegViewModel ModelView)
        {
            if (!ModelState.IsValid)
            {
                ViewBagAppointmentList();            
                return View(ModelView);
            }

            var Company = new AccountCompany(); // Entity Model
            var ExsistCompany = cntx_.AccountCompany.Any(xxx => xxx.ProducerId == ModelView.ProducerId);

            var UserExsist = cntx_.Account.Any(xxx => xxx.Login == ModelView.login);

            if (UserExsist)
            {
                ErrorMessage("Данный емаил уже зарегистрирован в нашей базе, попробуйте восстановить пароль");
                ViewBag.AppointmentList = cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                   .ToList()
                   .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                   .ToList();
                return View(ModelView);
            }

            if (ExsistCompany)
            {
                Company = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == ModelView.ProducerId).First();
            }
            else
            {
                Company.ProducerId = ModelView.ProducerId;
                Company.Name = ModelView.ProducerName;
                cntx_.Entry(Company).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();
                string DomainName = ModelView.login.ToString().Split(new Char[] { '@' }, StringSplitOptions.RemoveEmptyEntries)[1].ToLower();
                var X = new CompanyDomainName();
                X.Name = DomainName;
                X.CompanyId = Company.Id;
                cntx_.Entry(X).State = System.Data.Entity.EntityState.Added;
                cntx_.SaveChanges();
                Company.CompanyDomainName.Add(X);
                cntx_.SaveChanges();

            }

            cntx_.SaveChanges();

            string Password = GetRandomPassword();
            var NewAccount = SaveAccount(Reg_ViewModel: ModelView, Pass: Password, AC: Company);
            
            string AdminGroupName = GetWebConfigParameters("AdminGroupName");

            NewAccount.AccountGroup.Add(cntx_.AccountGroup.Where(xxx => xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser).First());
            cntx_.SaveChanges();

            ProducerInterfaceCommon.Heap.EmailSender.SendRegistrationMessage(cntx_, NewAccount.Id, Password, NewAccount.IP);

            SuccessMessage("Пароль отправлен на ваш email " + NewAccount.Login);
            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public ActionResult DomainRegistration(RegDomainViewModel modelAccount)
        {
         //   var ModelView = new RegisterDomainValidation();
            if (!ModelState.IsValid)
            {
                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == modelAccount.Producers).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();

                var ListAppointment = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
                ListAppointment.AddRange(cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                     .ToList()
                     .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                     .ToList());

                ViewBag.AppointmentList = ListAppointment;

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

            string Password = GetRandomPassword();
            var NewAccount = SaveAccount(RegDomain_ViewModel: modelAccount, Pass: Password);
                   
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

            SuccessMessage("Письмо с паролем отправлено на ваш email");
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

            ViewBagAppointmentList();       

            var ViewModel = new RegNotProducerViewModel();
            return View(ViewModel);
        }

        [HttpPost] 
        public ActionResult CustomRegistration(RegNotProducerViewModel AccountModel)
        {
            if (!ModelState.IsValid)
            {
                return View(AccountModel);
            }
            
            var CompanyCreate = new AccountCompany();

            CompanyCreate.Name = AccountModel.CompanyName; // Комментарий пользователя, далее название компании
            cntx_.Entry(CompanyCreate).State = EntityState.Added;
            cntx_.SaveChanges();

            var NewAccount = SaveAccount(RegNotProducer_ViewModel: AccountModel, AC: CompanyCreate);

            NewAccount.AccountCompany = CompanyCreate;
            cntx_.Entry(NewAccount).State = EntityState.Modified;
            cntx_.SaveChanges();
            
            SuccessMessage("Ваша заявка принята. Ожидвйте с вами свяжутся");
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public ActionResult AdminAuthentification(string AdminLogin, long? IdProducerUSer, string SecureHash)
        {

          if (AdminLogin == null || IdProducerUSer == null)
            {
                return RedirectToAction("index", "home");
            }

            // проверка наличия Админа В БД.

            var AccountAdminExsist = cntx_.Account.Any(xxx => xxx.Login == AdminLogin && xxx.TypeUser == (SByte)ProducerInterfaceCommon.ContextModels.TypeUsers.ControlPanelUser);
            var ProducerUserExsist = cntx_.Account.Any(xxx => xxx.Id == IdProducerUSer && xxx.TypeUser == (SByte)ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser);

            // проверка SecureHash

            var AdminAccount = cntx_.Account.Where(xxx => xxx.Login == AdminLogin).First();

            string i = "";

            if (AdminAccount.Name != null)
            {
                i = (AdminAccount.Name.Length * 19801112).ToString();
            }
            else
            {
                i = (18 * 19801112).ToString();
            }

            if (!SecureHash.Contains(i.ToString()))
            {
                return RedirectToAction("index", "home");
            }

            if (AccountAdminExsist && ProducerUserExsist)
            {
                return View("AdminAuth",new ProducerInterfaceCommon.ContextModels.AdminAutentification() { IdProducerUser = (long) IdProducerUSer, Login = AdminAccount.Login });
            }

            return RedirectToAction("index", "home");

        }

        [HttpPost]     
        public ActionResult AdminAuth(ProducerInterfaceCommon.ContextModels.AdminAutentification AdminAccountModel)
        {
            var DomainAuth = new ProducerInterfaceCommon.Controllers.DomainAutentification();
            if (DomainAuth.IsAuthenticated(AdminAccountModel.Login, AdminAccountModel.Password))
            {
                // авторизовываем как обычного пользователя, но с добавление ID Администратора 

                CurrentUser = cntx_.Account.Find(AdminAccountModel.IdProducerUser);
                var AdminId = cntx_.Account.Where(xxx => xxx.Login == AdminAccountModel.Login).First().Id.ToString();
                Autentificate(this, true, AdminId);
            }

            if (CurrentUser != null)
            {
                return RedirectToAction("Index", "Profile");
            }

            AdminAccountModel.Password = "";
            ErrorMessage("Пароль указан не верно");
            return View("AdminAuth", AdminAccountModel);
        }

        private Account SaveAccount(RegViewModel Reg_ViewModel = null, RegDomainViewModel RegDomain_ViewModel = null, RegNotProducerViewModel RegNotProducer_ViewModel = null, string Pass = null, AccountCompany AC = null)
        {
            
            var NewAccount = new Account();

            if (Reg_ViewModel != null)
            {
                NewAccount.Enabled = 0;
                NewAccount.CompanyId = AC.Id;
                NewAccount.Login = Reg_ViewModel.login;
                NewAccount.Name = Reg_ViewModel.LastName + " " + Reg_ViewModel.FirstName + " " + Reg_ViewModel.OtherName;
                NewAccount.LastName = Reg_ViewModel.LastName;
                NewAccount.FirstName = Reg_ViewModel.FirstName;
                NewAccount.OtherName = Reg_ViewModel.OtherName;
                NewAccount.Password = Md5HashHelper.GetHash(Pass);
                NewAccount.PasswordUpdated = SystemTime.GetDefaultDate();
                NewAccount.Phone = Reg_ViewModel.PhoneNumber;
                NewAccount.AppointmentId = Reg_ViewModel.AppointmentId;
                NewAccount.Appointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == Reg_ViewModel.AppointmentId).First().Name;
                NewAccount.TypeUser = (sbyte)TypeUsers.ProducerUser;  
            }

            if (RegDomain_ViewModel != null)
            {
                NewAccount.Login = RegDomain_ViewModel.Mailname + "@" + cntx_.CompanyDomainName.Where(xxx => xxx.Id == RegDomain_ViewModel.EmailDomain).First().Name;
                NewAccount.Name = RegDomain_ViewModel.LastName + " " + RegDomain_ViewModel.FirstName + " " + RegDomain_ViewModel.OtherName;
                NewAccount.LastName = RegDomain_ViewModel.LastName;
                NewAccount.FirstName = RegDomain_ViewModel.FirstName;
                NewAccount.OtherName = RegDomain_ViewModel.OtherName;
                NewAccount.TypeUser = (sbyte)TypeUsers.ProducerUser;
                NewAccount.PasswordUpdated = DateTime.MinValue;
                NewAccount.Enabled = 0;
                NewAccount.Appointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == RegDomain_ViewModel.AppointmentId).First().Name;
                NewAccount.CompanyId = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == RegDomain_ViewModel.Producers).First().Id;
                NewAccount.Password = Md5HashHelper.GetHash(Pass);
                NewAccount.Phone = RegDomain_ViewModel.PhoneNumber;  
            }

            if (RegNotProducer_ViewModel != null)
            {
                NewAccount.Login = RegNotProducer_ViewModel.login;
                NewAccount.Enabled = 0;
                NewAccount.TypeUser = (sbyte)TypeUsers.ProducerUser;
                NewAccount.Name = RegNotProducer_ViewModel.LastName + " " + RegNotProducer_ViewModel.FirstName + " " + RegNotProducer_ViewModel.OtherName;
                NewAccount.LastName = RegNotProducer_ViewModel.LastName;
                NewAccount.FirstName = RegNotProducer_ViewModel.FirstName;
                NewAccount.OtherName = RegNotProducer_ViewModel.OtherName;
                NewAccount.Appointment = RegNotProducer_ViewModel.Appointment;
                NewAccount.Phone = RegNotProducer_ViewModel.PhoneNumber;
                NewAccount.PasswordUpdated = DateTime.Now;
                NewAccount.LastUpdatePermisison = DateTime.Now;             
            }
            cntx_.Entry(NewAccount).State = System.Data.Entity.EntityState.Added;
            cntx_.SaveChanges();
            return NewAccount;
        }
    }
}