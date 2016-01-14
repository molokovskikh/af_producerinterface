using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices;
using System.Web.Security;
using System.Text;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base

        public Models.producerinterface_Entities cntx_ = new Models.producerinterface_Entities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string currentUser = GetUserName();

            if (currentUser != null)
            {
                ViewBag.currentUser = currentUser;
            }
            else
            {
                filterContext.Result = RedirectToAction("Index","Registration");
            }
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