using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using ProducerInterfaceCommon.ContextModels;

namespace ReportInterface.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        public ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx_ = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
   
        private string controllerAcctributes;
        private string controllerName;
        private string actionName;
        public string currentUser;
        private string permissionName {get { return (controllerName + "_" + actionName).ToLower(); } }
        
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            currentUser = GetUserName();
         
            controllerName = filterContext.Controller.GetType().Name.Replace("Controller", "").ToLower();
            actionName = filterContext.RouteData.Values["action"].ToString();

            // проверяем, авторизован ли пользователь

            if (currentUser != null)
            {
                ViewBag.currentUser = currentUser;

                CheckGlobalPermission(); // проверка наличия в данного экшена в БД
                var UserPermitionExsist = CheckUserPermission(); // проверка прав пользователя

                if (!UserPermitionExsist)
                {
                    ErrorMessage("У вас нет доступа к данной странице или для изменения данных");
                    //  Response.AddHeader("Status Code", "204");
                    filterContext.Result = RedirectToAction("Index", "Home");
                }
               
            }
            else
            {               
                if (controllerName != "registration")
                {
                    // Не авторизован, переводим на страницу авторизации
                    filterContext.Result = RedirectToAction("Index", "Registration");
                }
                // Не авторизован, уже заходит на страницу авторизции
            }     
            
             
        }

        public void CheckGlobalPermission()
        {
          
            var ValuesNames = Request.QueryString;
            var ValuesForm = Request.Form;

            controllerAcctributes = "";

            controllerAcctributes = Request.HttpMethod.ToString();

            bool PermitionExsist = false;


            if (controllerAcctributes != "")
            {
                PermitionExsist = cntx_.AccountPermission.Any(xxx => xxx.ControllerAction == permissionName && xxx.ActionAttributes.Contains(controllerAcctributes));
            }
            else
            {
                PermitionExsist = cntx_.AccountPermission.Any(xxx => xxx.ControllerAction == permissionName);
                controllerAcctributes = null;
            }
            

            if (!PermitionExsist)
            {
                // в базе не найдн пермишен для данного экшена

                var NewPermittion = new AccountPermission();
                NewPermittion.ActionAttributes = controllerAcctributes;
                NewPermittion.ControllerAction = permissionName;
                NewPermittion.Enabled = true;
                
                cntx_.AccountPermission.Add(NewPermittion);
                cntx_.SaveChanges();

                string AdminGroupname = GetWebConfigParameters("AdminGroupName");
                bool AdminGroupExsist = cntx_.AccountGroup.Any(xxx => xxx.Name == AdminGroupname);

                if (AdminGroupExsist)
                {
                    var AdminGroup = cntx_.AccountGroup.Where(xxx => xxx.Name == AdminGroupname).First();
                    AdminGroup.AccountPermission.Add(NewPermittion);
                    cntx_.SaveChanges();
                }
                else
                {
                    var NewAdminGroup = new AccountGroup();
                    NewAdminGroup.Enabled = true;
                    NewAdminGroup.Name = AdminGroupname;
                    cntx_.AccountGroup.Add(NewAdminGroup);
                    cntx_.SaveChanges();
                    NewAdminGroup.AccountPermission.Add(NewPermittion);
                    cntx_.SaveChanges();
                }
            }

            // пермишен добавлен в БАЗУ, если его не было, если был, то повторно не добавляется
            // так же он добавится если в базе Enabled  стоял false;

        }

        public bool CheckUserPermission()
        {       
            // проверяем наличие маршрута в Web.Config param key=IgnoreRoute
            // список игнорируемых маршрутов  "Controller_Action,Controller2_Action,Controller_Action2" 
            // и т.д.

            List<string> IgnoreRoute = GetWebConfigParameters("IgnoreRoute").Split(new Char[] { ',' }).ToList();

            foreach (string Permition in IgnoreRoute)
            {
                if (Permition.ToLower() == permissionName.ToLower())
                {
                    return true; // если пермишен найден в списке игнорируемых, возращаем true
                }
            }

            //bool UserPermitionExsist = false;

            // проверяем не состоит ли пользователь в группе 'Администраторы' название группы хранится в web.config key=AdminGroupName

            //string AdminGroupName = GetWebConfigParameters("AdminGroupName");

            //UserPermitionExsist = cntx_.controlpaneluserpermission.Any(xxx => xxx.Name == currentUser && xxx.GroupName == AdminGroupName);
            //if (UserPermitionExsist)
            //{
            //    return true; // пользователь состоит в группе Администраторы
            //}


            // если аттрибуты запроса пусты / нет в запросе передаваемых параметров
            if (controllerAcctributes == null)
            {
                return cntx_.AccountPermission.Where(vvv => vvv.ControllerAction == permissionName)
                .Any(xxx => xxx.AccountGroup.Any(yyy => yyy.Account.Any(zzz => zzz.Login == currentUser)));              
            }

            // в запросе есть передаваемые параметры



            return cntx_.AccountPermission.ToList().Where(vvv=>vvv.ControllerAction == permissionName && vvv.ActionAttributes.Contains(controllerAcctributes))
                .Any(xxx => xxx.AccountGroup.Any(yyy => yyy.Account.Any(zzz => zzz.Login == currentUser)));       

        }

        public string GetDescriptionThisAction()
        {
            return "";
        }


        public void ClearAllCookies(HttpContextBase context)
        {
            var cookiesName = "UserAutentification";
            for (int i = 0; i < context.Request.Cookies.Count; i++)
            {
                HttpCookie currentUserCookie = context.Request.Cookies[i];
                if (currentUserCookie != null && currentUserCookie.Name.IndexOf(cookiesName) != -1)
                {
                    context.Response.Cookies.Remove(currentUserCookie.Name);
                    currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                    currentUserCookie.Value = null;
                    context.Response.SetCookie(currentUserCookie);
                }
            }
        }

        public string GetLoginFromCookie()
        {
            string UserName = "";

            HttpCookie authCookie = this.Request.Cookies[GetWebConfigParameters("CockieName")];

            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                string cookiePath = ticket.CookiePath;
                DateTime expiration = ticket.Expiration;
                bool expired = ticket.Expired;
                bool isPersistent = ticket.IsPersistent;
                DateTime issueDate = ticket.IssueDate;
                string name = ticket.Name;
                string userData = ticket.UserData;
                string version = ticket.Version.ToString();
                UserName = name;
                authCookie.Expires = System.DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
                Response.Cookies.Set(authCookie);
            }
            else
            {
                UserName = null;
            }
            return UserName;
        }

        public void SetCookie(string name, string value)
        {
            if (value == null)
            {
                Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = DateTime.Now });
                return;
            }
            var plainTextBytes = Encoding.UTF8.GetBytes(value);
            var text = Convert.ToBase64String(plainTextBytes);
            Response.Cookies.Add(new HttpCookie(name, text) { Path = "/" });
        }

        public void DeleteCookie(string name)
        {
            Response.Cookies.Remove(name);
        }

        public void ClearAllCookies()
        {
            var cookiesName = GetWebConfigParameters("CockieName");
            for (int i = 0; i < Request.Cookies.Count; i++)
            {
                HttpCookie currentUserCookie = Request.Cookies[i];
                if (currentUserCookie != null && currentUserCookie.Name.IndexOf(cookiesName) != -1)
                {
                    Response.Cookies.Remove(currentUserCookie.Name);
                    currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                    currentUserCookie.Value = null;
                    Response.SetCookie(currentUserCookie);
                }
            }

        }

        public void SuccessMessage(string message)
        {
            SetCookie("SuccessMessage", message);
        }

        public void ErrorMessage(string message)
        {
            SetCookie("ErrorMessage", message);
        }

        public void WarningMessage(string message)
        {
            SetCookie("WarningMessage", message);
        }

        public string GetUserName()
        {
           return GetLoginFromCookie();
        }

        public string GetWebConfigParameters(string ParamKey)
        {
            return System.Configuration.ConfigurationManager.AppSettings[ParamKey].ToString();
        }

        public string DebugShedulerName()
        {
            string Name = "ServerScheduler";
#if DEBUG
            Name = "TestScheduler";
#endif
            return Name;
        }

     
    }

}