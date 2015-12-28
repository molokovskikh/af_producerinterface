using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ProducerInterface.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            ViewBag.JavascriptParams = new Dictionary<string, string>();
            var currentDate = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            AddJavascriptParam("Timestamp", currentDate.ToString());
        }

        protected static Models.producerinterface_Entities DB = new producerinterface_Entities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
         //   base.OnActionExecuting(filterContext);
            ViewBag.ActionName = filterContext.RouteData.Values["action"].ToString();
            ViewBag.ControllerName = GetType().Name.Replace("Controller", "");

            CurrentAnalitUser = AuthentificationModule.GetUser(this);
            string redirectUrl = AuthentificationModule.IsSecuredController(this, filterContext);

            if (redirectUrl != String.Empty)
            {
                filterContext.Result = new RedirectResult(redirectUrl, true);
            }                  
        }

        public produceruser CurrentAnalitUser { get; set; }

        protected void SetBreadcrumb(string str)
        {
            ViewBag.Breadcrumb = str;
        }

        public string GetBaseUrl()
        {
            var ret = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority,
                UrlHelper.GenerateContentUrl("~/", HttpContext));
            return ret;
        }

        public produceruser CurrentProduserUser { get; set; }

        protected ActionResult Authenticate(string username, bool shouldRemember, string userData = "")
        {
            return Redirect(AuthentificationModule.Authenticate(this, username, shouldRemember, userData));
        }
        public virtual void LogoutUser()
        {
            AuthentificationModule.ClearAllCookies(this.HttpContext);
        }

        public void RedirectUnAuthorizedUser(ActionExecutingContext filterContext, string redirectGlobalConfigRout = "",
        bool cleanCookies = true)
        {
            if (cleanCookies)
            {
                ProducerInterface.Models.AuthentificationModule.ClearAllCookies(this.HttpContext);
            }
            var unAuthorizedUserRout = "Home/Index";
            Url.Content(unAuthorizedUserRout);
            string[] url = unAuthorizedUserRout.Split('/');
            var controller = url[0];
            var action = url.Length > 1 ? url[1] : "Index";
            filterContext.Result = new RedirectResult(Url.Action(action, controller), true);
        }

        public string ActionName
        {
            get { return (string)ViewBag.ActionName; }
            set { }
        }    

        public void AddJavascriptParam(string name, string value)
        {
            ViewBag.JavascriptParams[name] = value;
        }
        public string GetJavascriptParam(string name)
        {
            string val = null;
            ViewBag.JavascriptParams.TryGetValue(name, out val);
            return val;
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

        protected string GetCookie(string cookieName)
        {
            try
            {
                var cookie = Request.Cookies.Get(cookieName);
                var base64EncodedBytes = Convert.FromBase64String(cookie.Value);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void SetCookie(string name, string value)
        {
            if (value == null)
            {
                Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = SystemTime.Now() });
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
        
        public class SystemTime
        {
            public static DateTime Now()
            {
                return DateTime.Now;
            }
        }

    }
}