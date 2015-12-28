using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProducerInterface.Controllers;
using System.Configuration;
using System.ComponentModel;
using System.Web.Http;
using System.Web.Security;
using System.Web.Mvc;

namespace ProducerInterface.Models
{
    public class AuthentificationModule
    {
        private static producerinterface_Entities DB = new producerinterface_Entities();

        public static string Authenticate(Controller currentController, string username,
        bool shouldRemember, string userData = "")
        {
            string cookiesName = "";
            try
            {
                cookiesName = System.Configuration.ConfigurationManager.AppSettings["CookieName"].ToString();
            }
            catch { cookiesName = "ValidationUserCookie"; }
            var redirectAfterAuthentication = "Profile/Index";
            string[] url = redirectAfterAuthentication.Split('/');
            var controller = url[0];
            var action = url.Length > 1 ? url[1] : "Index";
            
            var ticket = new FormsAuthenticationTicket(
                1,
                username,
                SystemTime.Now(),
                SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                shouldRemember,
                userData,
                FormsAuthentication.FormsCookiePath);

            var cookie = new HttpCookie(cookiesName, FormsAuthentication.Encrypt(ticket));
            if (shouldRemember)
                cookie.Expires = SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
            FormsAuthentication.SetAuthCookie(username, false);
            currentController.Response.Cookies.Set(cookie);

            return currentController.Url.Action(action, controller);
        }



        public static produceruser GetUser(BaseController curentController)
        {
            var currentUser = new produceruser();
            string cookiesName = "";
            try
            {
                cookiesName = System.Configuration.ConfigurationManager.AppSettings["CookieName"].ToString();
            }
            catch { cookiesName = "ValidationUserCookie"; }
            try
            {
                HttpCookie authCookie = curentController.Request.Cookies[cookiesName];
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
                    currentUser.Name = name;
                }
            }
            catch (Exception) { /*IGNORE*/ }
            return currentUser;
        }

        public static void CheckPermission<T>(ActionExecutingContext filterContext, T currentUser,
            string[] ignoreRouteForPermissionRedirect)
        {
            var actionName = filterContext.RouteData.Values["action"].ToString();
            var controllerName = filterContext.Controller.GetType().Name.Replace("Controller", "");

            string currentPermissionName = controllerName.ToLower() + "_" + actionName.ToLower();

            bool IgnoreRouteOrNotIgnore = IgnoreorNotIgnore(ignoreRouteForPermissionRedirect, currentPermissionName);

            if (!IgnoreRouteOrNotIgnore)
            {
                var baseController = (BaseController)filterContext.Controller;

                if (currentUser as produceruser == null)
                {
                    string url = IsSecuredController(baseController, filterContext, true);
                    if (url != String.Empty)
                    {
                        ClearAllCookies(baseController.HttpContext);
                        filterContext.Result = new RedirectResult(url);
                    }                  
                }

                if (currentUser as produceruser != null)
                {
                    var user = currentUser as produceruser;
                    var curentPermission = DB.userpermission.FirstOrDefault(s => s.Name.ToLower() == currentPermissionName);
                    if (currentUser != null && curentPermission != null && !CheckPermission(curentPermission))
                    {
                        baseController.ErrorMessage("У Вас нет прав доступа к запрашиваемой странице.");
                        baseController.RedirectUnAuthorizedUser(filterContext, cleanCookies: false);
                    }

                }
            }
        }

        public static bool IgnoreorNotIgnore(string[] ignoreRouteForPermissionRedirect, string ThisRouteString)
        {
            bool ret = false;  // по умолчанию маршрут не игнорируется
            if (ignoreRouteForPermissionRedirect == null || ignoreRouteForPermissionRedirect.Count() == 0)
            {
                return false; // список игнорируемых маршрутов пуст или равен нулю, данный маршрут не игнорируется
            }
            else
            {
                ret = ignoreRouteForPermissionRedirect.Any(xxx=>xxx==ThisRouteString);  
            }
            return ret;
        }

        internal static void ClearAllCookies(HttpContextBase httpContext)
        {
            var cookiesName = ConfigurationManager.AppSettings["CockieName"].ToString();
            for (int i = 0; i < httpContext.Request.Cookies.Count; i++)
            {
                HttpCookie currentUserCookie = httpContext.Request.Cookies[i];
                if (currentUserCookie != null && currentUserCookie.Name.IndexOf(cookiesName) != -1)
                {
                    httpContext.Response.Cookies.Remove(currentUserCookie.Name);
                    currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                    currentUserCookie.Value = null;
                    httpContext.Response.SetCookie(currentUserCookie);
                }
            }
        }

        public static IList<userpermission> GetUserPermissions()
        {
            var permissionsList = new List<userpermission>();
            //	Roles.ForEach(s => permissionsList.AddRange(s.Permissions));
            permissionsList.AddRange(permissionsList);
            return permissionsList;
        }

        public static bool CheckPermission(userpermission permission)
        {
            if (permission == null)
            {
                return true;
            }
            var permissionExists = GetUserPermissions().Any(s => s == permission);
            return permissionExists;
        }

        public static string IsSecuredController(Controller currentController, ActionExecutingContext filterContext, bool redirectAnyway = false)
        {
            var isToBeSecured = currentController.GetType().GetCustomAttributes(typeof(SecuredControllerAttribute), true).Any();
            var currentActionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            var currentControllerName = currentController.GetType().Name.Replace("Controller", "").ToLower();

            var unAuthorizedUserRedirectUrl = "Home/index";
            string[] urlUnAuthorizedUser = unAuthorizedUserRedirectUrl.Split('/');
            var controllerUnAuthorizedUser = urlUnAuthorizedUser[0].ToLower();
            var actionUnAuthorizedUser = urlUnAuthorizedUser.Length > 1 ? urlUnAuthorizedUser[1].ToLower() : "index";

            var RedirectAfterAuthenticationUrl = "Home/Index";
            string[] urlAfterAuthentication = RedirectAfterAuthenticationUrl.Split('/');
            var controllerAfterAuthentication = urlAfterAuthentication[0].ToLower();
            var actionAfterAuthentication = urlAfterAuthentication.Length > 1 ? urlAfterAuthentication[1].ToLower() : "index";

            var currentUserName = redirectAnyway
                ? ""
                : (currentController as BaseController != null
                   && (currentController as BaseController).CurrentAnalitUser != null
                    ? (currentController as BaseController).CurrentAnalitUser.Name
                    : "");

            if (isToBeSecured && string.IsNullOrEmpty(currentUserName) &&
                (currentActionName != actionUnAuthorizedUser
                 || (currentActionName != "" && actionUnAuthorizedUser == "index")
                 && currentControllerName != controllerUnAuthorizedUser))
            {
                return currentController.Url.Action(actionUnAuthorizedUser, controllerUnAuthorizedUser);
            }

            if (!string.IsNullOrEmpty(currentUserName) && currentActionName == actionUnAuthorizedUser &&
                currentControllerName == controllerUnAuthorizedUser)
            {
                return currentController.Url.Action(actionAfterAuthentication, controllerAfterAuthentication);
            }
            return "/Home/index";
        }

        public class SystemTime
        {
            public static DateTime Now()
            {
                return DateTime.Now;
            }
        }

    }
}