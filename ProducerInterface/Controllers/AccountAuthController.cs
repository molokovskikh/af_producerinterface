using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterface.Controllers
{
    public class AccountAuthController : MasterBaseController
    {

        [HttpPost]
        public ActionResult UserAuthentication(LoginValidation User_)
        {

            if (String.IsNullOrEmpty(User_.login) && String.IsNullOrEmpty(User_.password))
            {
                ErrorMessage("Некорректно введены данные.  Вашим логином является Email, указанный при регистрации. Пароль при регистрации был выслан на ваш Email");
                ViewBag.CurrentUser = null;
                return RedirectToAction("Index", "Home");
            }

            // валидация

            var ThisUser = cntx_.Account.ToList().Where(xxx => xxx.Login.ToLower() == User_.login.ToLower() && xxx.TypeUser == SbyteTypeUser).FirstOrDefault();

            if (ThisUser == null || ThisUser.Login == "")
            {
                ErrorMessage("Пользователь с данным Логином не существует. Вашим логином является Email, указанный при регистрации.");
                ViewBag.CurrentUser = null;
                return RedirectToAction("Index", "Home");
            }

            // проверка наличия в БД
            
            string originalPass = User_.password;
            User_.password = Md5HashHelper.GetHash(User_.password);

            if (User_.password != ThisUser.Password)
            {
                ErrorMessage("Неправильно введен пароль.");
                ViewBag.CurrentUser = null;
                return RedirectToAction("Index", "Home");
            }

            // проверка пароля

            // если всё выше перечисленное пройдено,
            // проверяем первый раз логинится пользователь или второй или после смены пароля
            
            if (ThisUser.Enabled == 1) // логинится не впервый раз и не заблокирован
            {

                CurrentUser = ThisUser as Account;
                return Autentificate(this, shouldRemember: true);
            }


            if (ThisUser.Enabled == 0 && (ThisUser.PasswordUpdated.Value == SystemTime.GetDefaultDate())) // логинится впервые
            {

                var ListUser = cntx_.Account.Where(xxx => xxx.CompanyId == ThisUser.CompanyId.Value).Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser).ToList();

                List<AccountPermission> LST = cntx_.AccountPermission.ToList();

                if (ListUser == null || ListUser.Count() == 0)
                {
                    // данный пользователь зарегистрировался первым, даём ему все права

                    // пользователь зарегистрировался первым, добавляем его в группу администраторов

                    string AdminGroupName = GetWebConfigParameters("AdminGroupName");

                    // проверяем наличие группы администраторов


                    var AdminGroup = cntx_.AccountGroup.Where(xxx => xxx.Name == AdminGroupName).FirstOrDefault();

                    if (String.IsNullOrEmpty(AdminGroup.Name))
                    {
                        AdminGroup = new AccountGroup();

                        AdminGroup.Name = AdminGroupName;
                        AdminGroup.Enabled = true;
                        AdminGroup.Description = "Администраторы";
                        AdminGroup.TypeGroup = SbyteTypeUser;
                        cntx_.Entry(AdminGroup).State = System.Data.Entity.EntityState.Added;
                        cntx_.SaveChanges();
                    }

                    AdminGroup.Account.Add(ThisUser);
                    cntx_.SaveChanges();
                }
                else
                {

                    // Даём ему права для входа в ЛК
                    // LogonGroupAcess 

                    var GroupName = GetWebConfigParameters("LogonGroupAcess");

                    var OtherGroup = cntx_.AccountGroup.Where(xxx => xxx.Name == GroupName && xxx.TypeGroup == SbyteTypeUser).FirstOrDefault();


                    if (String.IsNullOrEmpty(OtherGroup.Name))
                    {
                        OtherGroup = new AccountGroup();

                        OtherGroup.Name = GroupName;
                        OtherGroup.Enabled = true;
                        OtherGroup.Description = "Администраторы";
                        OtherGroup.TypeGroup = SbyteTypeUser;
                        cntx_.Entry(OtherGroup).State = System.Data.Entity.EntityState.Added;
                        cntx_.SaveChanges();
                    }

                    OtherGroup.Account.Add(ThisUser);
                    cntx_.SaveChanges();
                }
                SuccessMessage("Вы успешно подтвердили свою регистрацию на сайте");

                ThisUser.PasswordUpdated = SystemTime.Now();
                ThisUser.Enabled = 1;
                cntx_.Entry(ThisUser).State = System.Data.Entity.EntityState.Modified;

                cntx_.SaveChanges();

                //CurrentUser = ThisUser;
                //ViewBag.CurrentUser = ThisUser;
                CurrentUser = ThisUser;
                return Autentificate(this, shouldRemember: true);
            }

            if (ThisUser.Enabled == 0 && ThisUser.PasswordUpdated != null)
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
                    CurrentUser = ThisUser;
                    return Autentificate(this, shouldRemember: true);
                }
                else
                {
                    // Аккаунт заблокирован

                    ErrorMessage("Аккаунт заблокирован");
                    //CurrentUser = null;
                    //ViewBag.CurrentUser = null;
                    CurrentUser = null;
                    ErrorMessage("Аккаунт заблокирован");
                    return RedirectToAction("Index", "Home");
                }
            }


            // CurrentUser = null;
            // ViewBag.CurrentUser = null;
            // AutorizedUser = null;
            return RedirectToAction("Index", "Home");
        }
        
        public ActionResult Autentificate(Controller currentController, bool shouldRemember, string userData = "")
        {
            string autorizeddd = Autentificates(this, CurrentUser.Login, shouldRemember, userData);
            string controllerName = (autorizeddd.Split(new Char[] { '/' }))[0];
            string actionName = (autorizeddd.Split(new Char[] { '/' }))[1];
            return RedirectToAction(actionName, controllerName);
        }

        public string Autentificates(Controller CRT, string username, bool shouldRemember, string userData = "")
        {
            string CoockieName = GetWebConfigParameters("CookiesName");

            var redirectAfterAuthentication = "Home/Index";
            string[] url = redirectAfterAuthentication.Split('/');
            var controller = url[0];
            var action = url.Length > 1 ? url[1] : "Index";

            var ticket = new FormsAuthenticationTicket(
                1,
                this.CurrentUser.Login,
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
            FormsAuthentication.SetAuthCookie(CurrentUser.Name, false);
            Response.Cookies.Set(cookie);

            return "Profile/index";
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
                var User = new ProducerInterfaceCommon.ContextModels.Account();

                try
                {
                    User = cntx_.Account.Where(xxx => xxx.Login == login && xxx.TypeUser == 0).ToList().FirstOrDefault();
                }
                catch
                {
                    User = null;
                }

                if (User != null && User.Login != "")
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

                        ProducerInterfaceCommon.Heap.EmailSender.SendPasswordRecoveryMessage(cntx_, User.Id, Password, Request.UserHostAddress.ToString());

                        SuccessMessage("Новый пароль отправлен на ваш email: " + login);
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

                            SuccessMessage("Новый пароль отправлен на ваш email: " + login);

                            ProducerInterfaceCommon.Heap.EmailSender.SendPasswordRecoveryMessage(cntx_, User.Id, Password, Request.UserHostAddress.ToString());

                            return RedirectToAction("Index", "Home");
                            // отсылаем новвый пароль на почту
                        }
                        else
                        {
                            ErrorMessage("Ваша учетная запись Заблокирована, для обращений используйте email " + System.Configuration.ConfigurationManager.AppSettings["ProducerInterfaceForwardEmail"].ToString());
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    // пользователь не найден
                    // отсылаем на домашнюю, с ошибкой

                    ErrorMessage("Пользователь с email " + login + " не найден в базе или заблокирован , для обращений используйте email " + System.Configuration.ConfigurationManager.AppSettings["ProducerInterfaceForwardEmail"].ToString());
                    return RedirectToAction("PasswordRecovery", "Registration");

                }
            }
        }

        public ActionResult ChangePassword()
        {
            var User = cntx_.Account.Where(xxx => xxx.Login == CurrentUser.Login && xxx.TypeUser == 0).FirstOrDefault();
            string password = GetRandomPassword();
            User.Password = Md5HashHelper.GetHash(password);

            cntx_.Entry(User).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Новый пароль отправлен на ваш email: " + User.Login);

            ProducerInterfaceCommon.Heap.EmailSender.SendPasswordChangeMessage(cntx: cntx_, userId: User.Id, password: password, ip: Request.UserHostAddress);

            return RedirectToAction("Index", "Profile");
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