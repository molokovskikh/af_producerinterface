using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices;
using System.Web.Security;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class RegistrationController : MasterBaseController
    {
        // GET: Registration
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string login, string password)
        {
            if (IsAuthenticated(login, password))
            {
                var X = cntx_.ProducerUser.Where(xxx => xxx.Login == login && xxx.TypeUser ==1).FirstOrDefault();

                if(X == null)
                {
                    //  AddUserInDB(login);  // добавление обычного пользователя, у которого не будет прав
                    AddAdminUserInBD(login); // добавление администратора
                }

                SetLoginCookie((object)login);    
                return RedirectToAction("Index", "Home");                
            }
            ErrorMessage(ErrorMessageString);
            return RedirectToAction("Index", "Registration");
        }

        public void AddUserInDB(string LogIn)
        {
            ProducerUser CPU = new ProducerUser();

            CPU.Login = LogIn;
            CPU.Enabled = 1;
            CPU.Email = "";
            CPU.UserType = TypeUsers.ControlPanelUser;
            CPU.Appointment = "";

            if (_filterAttribute != null && _filterAttribute != "" && _filterAttribute.Length > 10)
            {
                CPU.Name = _filterAttribute;
            }

            cntx_.ProducerUser.Add(CPU);

            cntx_.SaveChanges();

            string AdminGroup = GetWebConfigParameters("Все");

            long IdGroup = cntx_.ControlPanelGroup.Where(xxx => xxx.Name == AdminGroup).FirstOrDefault().Id;

            if (IdGroup > 0)
            {
                var Group = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == IdGroup).First();
                Group.ProducerUser.Add(CPU);
                cntx_.SaveChanges();
            }
            else
            {
                var Group = new ControlPanelGroup();
                Group.Name = AdminGroup;
                Group.ProducerUser.Add(CPU);
            }

            cntx_.SaveChanges();

        }

        public void AddAdminUserInBD(string LogIn)
        {
            ProducerUser CPU = new ProducerUser();

            // CPU - ControlPanelUser

            CPU.Login = LogIn;            
            CPU.Enabled = 1;
            CPU.UserType = TypeUsers.ControlPanelUser;
            CPU.Email = "";
            CPU.Appointment = "";

            if (_filterAttribute != null && _filterAttribute != "" && _filterAttribute.Length > 10)
            {
                CPU.Name = _filterAttribute;
            }

            cntx_.ProducerUser.Add(CPU);

            cntx_.SaveChanges();

            string AdminGroup = GetWebConfigParameters("AdminGroupName");

            var GroupExsist = cntx_.ControlPanelGroup.Any(xxx => xxx.Name == AdminGroup);

            if (GroupExsist)
            {
                var Group = cntx_.ControlPanelGroup.Where(xxx => xxx.Name == AdminGroup).First();
                Group.ProducerUser.Add(CPU);             
            }
            else
            {
                var Group = new ControlPanelGroup();
                Group.Name = AdminGroup;
                Group.Enabled = true;                
                cntx_.ControlPanelGroup.Add(Group);
                cntx_.SaveChanges();
                Group.ProducerUser.Add(CPU);             
            }

            cntx_.SaveChanges();

        }

        private DirectoryEntry entryAu;
        private string _path;
        private string _filterAttribute;
        public string ErrorMessageString;        
        //"LDAP://OU=Клиенты,DC=adc,DC=analit,DC=net"

        public bool IsAuthenticated(string username, string pwd)
        {
            if (Authenticated(@"LDAP://OU=Офис,DC=adc,DC=analit,DC=net", username, pwd))
                return true;
            if (Authenticated(@"LDAP://OU=Клиенты,DC=adc,DC=analit,DC=net", username, pwd))
                return true;
            return false;
        }

        public bool Authenticated(string LDAP, string username, string pwd)
        {
            var domainAndUsername = @"analit\" + username;
            entryAu = new DirectoryEntry(LDAP, domainAndUsername, pwd, AuthenticationTypes.None);
            try
            {
                // Bind to the native AdsObject to force authentication.
                var obj = entryAu.NativeObject;
                var search = new DirectorySearcher(entryAu);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
          
                SearchResult result = search.FindOne();
                // Update the new path to the user in the directory
                _path = result.Path;
                _filterAttribute = (String)result.Properties["cn"][0];              
            }
            catch (Exception ex)
            {
                //_log.Info("Пароль или логин был введен неправильно");
                //_log.Info(ErrorMessage);
                ErrorMessageString = ex.Message;
                return false;
            }
            entryAu.RefreshCache();
            return true;
        }


        private DirectoryEntry GetDirectoryEntry(string login)
        {
            var entry = FindDirectoryEntry(login);
            if (entry == null)
                throw new Exception(String.Format("Учетная запись Active Directory {0} не найдена", login));
            return entry;
        }

        public void ChangePassword(string login, string password)
        {
            var entry = GetDirectoryEntry(login);
            GetDirectoryEntry(login).Invoke("SetPassword", password);
            entry.CommitChanges();
        }

        public void CreateUserInAD(string login, string password, bool allComputers = false)
        {
#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + login, "user");
			user.Properties["samAccountName"].Value = login;
			//user.Properties["description"].Value = clientCode.ToString();
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.Properties["userAccountControl"].Value = 66048;
			user.CommitChanges();
			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();
			root.CommitChanges();
#endif
        }

        public DirectoryEntry FindDirectoryEntry(string login)
        {
            using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(sAMAccountName={0}))", login)))
            {
                var searchResult = searcher.FindOne();
                if (searchResult != null)
                    return searcher.FindOne().GetDirectoryEntry();
                return null;
            }
        }

        public void SetLoginCookie(object login)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                login.ToString(),
                DateTime.Now,
                DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                false,
                "",
                FormsAuthentication.FormsCookiePath);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.Name = GetWebConfigParameters("CockieName");
            Response.Cookies.Set(cookie);
        }
    }
}