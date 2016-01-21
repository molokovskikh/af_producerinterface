using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using EntityContext.ContextModels;

namespace ProducerInterface.Controllers.pruducercontroller
{
    public class ConfigurationController : Controller
    {
        public EntityContext.ContextModels.producerinterface_Entities  cntx_ = new producerinterface_Entities();
        public ProducerUser AutorizedUser {  get;  set; }
        public ProducerUser CurrentUser { get; set; }

        public string GetCoockieName { get { return "ValidationUserCookie"; } }
        public string GetunAuthorizedUserRedirectUrl { get { return "Home/index"; }  }
        public string GetredirectAfterAuthentication { get { return "Profile/index"; } }

        public int MaxPasswordLeight {
            get { return 6; }
        }

        public string GetSiteName { get { return "producerinterface.analit.net"; } }

        public string ForwardEmail { get { return "office@analit.net"; }}

        protected override void OnException(ExceptionContext filterContext)
		{		
			DeleteCookie("SuccessMessage");
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

        public string GetCookie(string cookieName)
        {
            try
            {
                var cookie = Request.Cookies.Get(cookieName);
                var base64EncodedBytes = Convert.FromBase64String(cookie.Value);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception e)
            {
                //IGNORE
                var x = e;
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

        public void ClearAllCookies()
        {
            var cookiesName = System.Configuration.ConfigurationManager.AppSettings["CockieName"].ToString();
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
    }
}