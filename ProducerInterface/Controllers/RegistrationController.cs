using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EntityContext.ContextModels;

namespace ProducerInterface.Controllers
{
    public class RegistrationController : pruducercontroller.BaseController
    {
        private Quartz.Job.EDM.reportData cntx;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            cntx = new Quartz.Job.EDM.reportData();
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
        }

        // GET: Registration
        public ActionResult Index()
        {
            var ModelView = new LoginValidation();
            return View(ModelView);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            var ModelView = new RegistrerValidation();            
            Quartz.Job.NamesHelper h;            
            h = new Quartz.Job.NamesHelper(cntx, 0);                     
            ViewBag.ProducerList = h.GetProducerList(); 

            return View();
        }
        
        [HttpPost]
        public ActionResult Registration(RegistrerValidation NewAccount = null)
        {         

            if (ModelState.IsValid)
            {
                // проверка на существование eMail в БД

                var YesNouMail = cntx_.ProducerUser.Where(xxx => xxx.Email == NewAccount.login && xxx.Login == null).FirstOrDefault();

                if (YesNouMail == null && NewAccount.login != "")
                {
                    ProducerUser New_User = new ProducerUser();
                    New_User.Email = NewAccount.login;
                    New_User.Name = NewAccount.Name;
                    New_User.Enabled = 0;
                    New_User.Appointment = NewAccount.Appointment;
                    string Password = GetRandomPassword();
                    New_User.Password = Md5HashHelper.GetHash(Password);
                    New_User.ProducerId = NewAccount.Producers;
                    New_User.PasswordUpdated = SystemTime.GetDefaultDate();
                    cntx_.Entry(New_User).State = System.Data.Entity.EntityState.Added;
                    cntx_.SaveChanges();

                    string eMail = New_User.Email;

                    New_User = cntx_.ProducerUser.Where(xxx => xxx.Email == eMail && xxx.Login == null).First();
                    
                    Quartz.Job.EmailSender.SendRegistrationMessage(cntx, New_User.Id, Password, Request.UserHostAddress.ToString());

                    SuccessMessage("Регистрация прошла успешно! Пароль для входа выслан на вашу почту " + NewAccount.login);

                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    // eMail уже зарегистрирован в Базе (Активный он или нет, не важно.)
                    // т.к. Заблокированный не сможет второй раз зарегистрироватся

                    string Errorstr = "Данный eMail уже зарегистрирован, попробуйте восстановить пароль.";

                    ErrorMessage(Errorstr);
                    return RedirectToAction("PasswordRecovery", "Registration", new { eMail = NewAccount.login });
                }
            }
            else
            {
                string Errorstr = "Некорректно заполнены поля формы / или не все";
                ErrorMessage(Errorstr);
                Quartz.Job.NamesHelper h;
                h = new Quartz.Job.NamesHelper(cntx, 0);
                ViewBag.ProducerList = h.GetProducerList();
                return View(NewAccount);

               // return RedirectToAction("Registration", "Registration");
            }         
        }
               
        public ActionResult PasswordRecovery(string eMail = "")
        {
            var Login = new PasswordUpdate();
            Login.login = eMail;        
            return View();
        }

        [HttpPost]
        public ActionResult PasswordRecoverySendMail(string login = "")
        {
            if (String.IsNullOrEmpty(login))
            {
                ErrorMessage("Вы не указали eMail");
                return RedirectToAction("PasswordRecovery", "Registration");
            }
            else
            {
                var User = new EntityContext.ContextModels.ProducerUser();

                try
                {
                    User = cntx_.ProducerUser.Where(xxx => xxx.Email == login).ToList().Where(xxx=>xxx.Login == null).FirstOrDefault();
                }
                catch
                {
                    User = null;
                }

                if (User != null && User.Email != "")
                {

                    // проверка не заблокирован ли пользователь
                    if (User.Enabled == 0 && User.PasswordUpdated == null)
                    {
                        // пользователь зарегистрировался, но ни разу не входил
                        // отсылаем новвый пароль на почту

                        string Password = GetRandomPassword();
                        User.Password = Md5HashHelper.GetHash(Password);
                        cntx_.Entry(User).State = System.Data.Entity.EntityState.Modified;
                        cntx_.SaveChanges();

                        Quartz.Job.EmailSender.SendPasswordRecoveryMessage(cntx, User.Id, Password, Request.UserHostAddress.ToString());

                        SuccessMessage("Новый пароль отправлен на ваш почтовый ящик: " + login);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        if (User.Enabled == 1)
                        {
                            string Password = GetRandomPassword();
                            User.Password = Md5HashHelper.GetHash(Password);
                            cntx_.Entry(User).State = System.Data.Entity.EntityState.Modified;
                            cntx_.SaveChanges();

                            SuccessMessage("Новый пароль отправлен на ваш почтовый ящик: " + login);

                            Quartz.Job.EmailSender.SendPasswordRecoveryMessage(cntx, User.Id, Password, Request.UserHostAddress.ToString());

                            return RedirectToAction("Index", "Home");
                            // отсылаем новвый пароль на почту
                        }
                        else
                        {
                            ErrorMessage("Ваша учетная запись Заблокирована, для обращений используйте адрес " + System.Configuration.ConfigurationManager.AppSettings["ProducerInterfaceForwardEmail"].ToString());
                            return RedirectToAction("Index","Home");
                        }
                    }
                }
                else
                {
                    // пользователь не найден
                    // отсылаем на домашнюю, с ошибкой

                    ErrorMessage("Пользователь с почтовым ящиком не найден в базе или заблокирован , для обращений используйте адрес " + System.Configuration.ConfigurationManager.AppSettings["ProducerInterfaceForwardEmail"].ToString());
                    return RedirectToAction("PasswordRecovery", "Registration");

                }
            }         
        }
             
        public ActionResult ChangePassword()
        {
            var User = cntx_.ProducerUser.Where(xxx => xxx.Email == CurrentUser.Email && xxx.Login == null).FirstOrDefault();
            string password = GetRandomPassword();
            User.Password = Md5HashHelper.GetHash(password);

            cntx_.Entry(User).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Новый пароль отправлен на ваш почтовый ящик: " + User.Email);

						Quartz.Job.EmailSender.SendPasswordChangeMessage(cntx: cntx, userId: User.Id, password: password, ip: Request.UserHostAddress);

            return RedirectToAction("Index", "Profile");
        }
        
        [HttpPost]
        public ActionResult UserAuthentication(LoginValidation User_)
        {

            if (String.IsNullOrEmpty(User_.login) && String.IsNullOrEmpty(User_.password))
            {
                ErrorMessage("Некорректно введены данные.");
                ViewBag.CurrentUser = null;
                return RedirectToAction("Index", "Home");
            }

            // валидация

            var ThisUser = cntx_.ProducerUser.ToList().Where(xxx => xxx.Email.ToLower() == User_.login.ToLower() && xxx.Login == null).FirstOrDefault();

            if (ThisUser == null || ThisUser.Email == "")
            {
                ErrorMessage("Пользователь с данным Логином не существует.");
                ViewBag.CurrentUser = null;
                return RedirectToAction("Index", "Home");
            }

            // проверка наличия в БД


            string originalPass = User_.password;            
            User_.password = Md5HashHelper.GetHash(User_.password);

            if (User_.password != ThisUser.Password)
            {
                ErrorMessage("Неправильно введён пароль.");
                ViewBag.CurrentUser = null;
                return RedirectToAction("Index", "Home");
            }

            // проверка пароля

            // если всё выше перечисленное пройдено,
            // проверяем первый раз логинится пользователь или второй или после смены пароля
        

            if (ThisUser.Enabled == 1) // логинится не впервый раз и не заблокирован
            {
         
                //CurrentUser = ThisUser;
                //ViewBag.CurrentUser = ThisUser as produceruser;
                AutorizedUser = ThisUser as ProducerUser;
                return Autentificate(this, shouldRemember: true);
            }
                 

            if (ThisUser.Enabled == 0 && (ThisUser.PasswordUpdated.Value == SystemTime.GetDefaultDate())) // логинится впервые
            { 
                
                var ListUser = cntx_.ProducerUser.Where(xxx => xxx.ProducerId == ThisUser.ProducerId.Value).Where(xxx => xxx.Enabled == 1).ToList();

                List<UserPermission> LST = cntx_.UserPermission.ToList();

                if (ListUser == null || ListUser.Count() == 0)
                {
                    // данный пользователь зарегистрировался первым, даём ему все права

                    foreach (var X in LST)
                    {
                        var AddOnePermission = new usertouserrole();
                        AddOnePermission.ProducerUserId = ThisUser.Id;
                        AddOnePermission.UserPermissionId = X.Id;
                        cntx_.Entry(AddOnePermission).State = System.Data.Entity.EntityState.Added;
                    }

                }
                else
                {

                    // Даём ему права для входа в ЛК

                    var AddOnePermission = new usertouserrole();
                    AddOnePermission.ProducerUserId = ThisUser.Id;


                    
                    AddOnePermission.UserPermissionId = LST.Where(xxx => xxx.Name.ToLower() == ("Profile_Index").ToLower()).First().Id;
                    cntx_.Entry(AddOnePermission).State = System.Data.Entity.EntityState.Added;
                }
                SuccessMessage("Вы успешно подтвердили свою регистрацию на сайте");

                ThisUser.PasswordUpdated = SystemTime.Now();
                ThisUser.Enabled = 1;                
                cntx_.Entry(ThisUser).State = System.Data.Entity.EntityState.Modified;

                cntx_.SaveChanges();

                 //CurrentUser = ThisUser;
                 //ViewBag.CurrentUser = ThisUser;
                 AutorizedUser = ThisUser;
                return Autentificate(this, shouldRemember: true);
            }

            if(ThisUser.Enabled == 0 && ThisUser.PasswordUpdated != null)
            {
                if (ThisUser.PasswordUpdated == SystemTime.GetDefaultDate())
                {
                    // Восстановление пароля

                    ThisUser.Enabled = 1;
                    ThisUser.PasswordUpdated = SystemTime.Now();
                    cntx_.Entry(ThisUser).State = System.Data.Entity.EntityState.Modified;
                    cntx_.SaveChanges();

                    //CurrentUser = ThisUser;
                    //ViewBag.CurrentUser = ThisUser;
                    AutorizedUser = ThisUser;
                    return Autentificate(this, shouldRemember: true);
                }
                else
                {
                    // Аккаунт заблокирован

                    ErrorMessage("Аккаунт заблокирован");
                    //CurrentUser = null;
                    //ViewBag.CurrentUser = null;
                    AutorizedUser = null;
                    ErrorMessage("Аккаунт заблокирован");
                    return RedirectToAction("Index", "Home");
                }            
            }
              

            // CurrentUser = null;
            // ViewBag.CurrentUser = null;
            // AutorizedUser = null;
            return RedirectToAction("Index","Home");
        }

        public ActionResult Autentificate(Controller currentController, bool shouldRemember, string userData = "")
        {
            string autorizeddd = Autentificates(this, AutorizedUser.Email, shouldRemember, userData);

            string controllerName = (autorizeddd.Split(new Char[] { '/' }))[0];
            string actionName = (autorizeddd.Split(new Char[] { '/' }))[1];
            return RedirectToAction(actionName, controllerName);
        }

        public string Autentificates(Controller CRT, string username, bool shouldRemember, string userData = "")
        {
            string CoockieName = GetCoockieName;

            var redirectAfterAuthentication = GetredirectAfterAuthentication;
            string[] url = redirectAfterAuthentication.Split('/');
            var controller = url[0];
            var action = url.Length > 1 ? url[1] : "Index";

            var ticket = new FormsAuthenticationTicket(
                1,
                this.AutorizedUser.Email,
                SystemTime.Now(),
                SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                shouldRemember,
                userData,
                FormsAuthentication.FormsCookiePath
                );

            var cookie = new HttpCookie(CoockieName, FormsAuthentication.Encrypt(ticket));

            if (shouldRemember)
            {
                cookie.Expires = SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
            }
            FormsAuthentication.SetAuthCookie(AutorizedUser.Name, false);
            Response.Cookies.Set(cookie);

            return GetredirectAfterAuthentication;
        }

        public ActionResult LogOut()
        {
            // зануляем куки регистрации формой
            LogOut_User();
            // возвращаем пользователя на главную страницу
            return RedirectToAction("Index", "Home");

        }
    }
}