using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ProducerInterfaceCommon.Controllers
{
    public class GlobalController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //protected string controllerName;
            //protected string actionName;
            //protected string controllerAcctributes;
            controllerName = this.GetType().Name.Replace("Controller", "").ToLower().ToString();
            actionName = this.Request.RequestContext.RouteData.GetRequiredString("action").ToLower().ToString();
            controllerAcctributes = this.Request.HttpMethod.ToString().ToLower();            
        }

        // глобальные переменные и функции
        // тип текущего пользователя SByte - хранится в ProducerUser таблице, параметр TypeUser

        protected ContextModels.TypeUsers TypeLoginUser { get { return (ContextModels.TypeUsers)SbyteTypeUser; } set { SbyteTypeUser = (SByte)value; } }
        protected SByte SbyteTypeUser { get; set; }

        // сюда попадёт авторизованный пользователь
        protected ContextModels.Account CurrentUser { get; set; }
        protected long CurrentUserIdLog { get; set; }

        // Context DataBase       
        protected ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx_ = new ContextModels.producerinterface_Entities();
        
        protected string controllerName;
        protected string actionName;
        protected string controllerAcctributes;

        protected string permissionName { get { return (controllerName + "_" + actionName).ToLower(); } }

        public string GetRandomPassword()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToLower().Substring(8, MaxPasswordLeight);
        }

        public class Md5HashHelper
        {
            /// <summary>
            /// Получение хэша строки
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public static string GetHash(string text)
            {
                using (MD5 md5Hash = MD5.Create()) return GetMd5Hash(md5Hash, text);
            }

            /// <summary>
            /// получение хэша строки
            /// </summary>
            /// <param name="md5Hash">Объект MD5</param>
            /// <param name="text">Хэшируемая строка</param>
            /// <returns>Хыш строки</returns>
            public static string GetMd5Hash(MD5 md5Hash, string text)
            {
                // Конвертация байтового массива в хэш
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                // создание строки
                StringBuilder sBuilder = new StringBuilder();
                // проходит по каждому байту хэша и форматирует его в 16 string
                for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
                return sBuilder.ToString();
            }

        }
        
        private int MaxPasswordLeight
        {
            get { return 6; }
        }

        public class SystemTime
        {
            public static DateTime Now()
            {
                return DateTime.Now;
            }
            public static DateTime GetDefaultDate()
            {
                return default(DateTime);

            }

        }

        public string GetUserCookiesName()
        {
            var currentUser = "";

            string cookiesName = GetWebConfigParameters("CookiesName");

            try
            {
                HttpCookie authCookie = Request.Cookies[cookiesName];
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
                    currentUser = name;
                    if (String.IsNullOrEmpty(ticket.UserData))
                    { }
                    else
                    {
                        CurrentUserIdLog = Convert.ToInt64(ticket.UserData);
                    }                 
                    authCookie.Expires = DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
                    Response.Cookies.Set(authCookie);
                }
            }
            catch (Exception) { /*IGNORE*/ }

            return currentUser;
        }
                
        #region COOKIES
        public void SetUserCookiesName(string UserLoginOrEmail, bool shouldRemember = true, string userData= "")
        {
            var CoockieName = GetWebConfigParameters("CookiesName");
            var ticket = new FormsAuthenticationTicket(
            1,
            UserLoginOrEmail,
            DateTime.Now,
            DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
            shouldRemember,
            userData,
            FormsAuthentication.FormsCookiePath
            );

            var cookie = new HttpCookie(CoockieName, FormsAuthentication.Encrypt(ticket));

            if (shouldRemember)
            {
                cookie.Expires = DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
            }

            FormsAuthentication.SetAuthCookie(UserLoginOrEmail, false);
            Response.Cookies.Set(cookie);
        }

        public void SetCookie(string name, string value)
        {
            if (value == null)
            {
                Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = DateTime.Now });
                return;
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var text = Convert.ToBase64String(plainTextBytes);
            Response.Cookies.Add(new HttpCookie(name, text) { Path = "/" });
        }

        public void DeleteCookie(string name)
        {
            Response.Cookies.Remove(name);
        }

        public void LogOut_User()
        {
            // очишаем куки, пользователь более не аутентифицирован
            ClearAllCookies();
        }

        public void ClearAllCookies()
        {
            var cookiesName = GetWebConfigParameters("CookiesName");
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

        public string GetWebConfigParameters(string ParamKey)
        {
            return System.Configuration.ConfigurationManager.AppSettings[ParamKey].ToString();
        }

        #endregion


     
    }
}
