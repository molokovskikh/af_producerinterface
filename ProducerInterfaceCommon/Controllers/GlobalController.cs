using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;

namespace ProducerInterfaceCommon.Controllers
{
	public class GlobalController : Controller
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			controllerName = this.GetType().Name.Replace("Controller", "").ToLower();
			actionName = this.Request.RequestContext.RouteData.GetRequiredString("action").ToLower();
			controllerAcctributes = this.Request.HttpMethod.ToLower();
		}

		// глобальные переменные и функции
		// тип текущего пользователя SByte - хранится в ProducerUser таблице, параметр TypeUser

		protected TypeUsers TypeLoginUser { get { return (TypeUsers)SbyteTypeUser; } set { SbyteTypeUser = (SByte)value; } }
		protected SByte SbyteTypeUser { get; set; }

		// сюда попадёт авторизованный пользователь
		protected ContextModels.Account CurrentUser { get; set; }
		protected long CurrentUserIdLog { get; set; }

		// Context DataBase       
		protected producerinterface_Entities DB = new ContextModels.producerinterface_Entities();

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

		public string GetUserCookiesName()
		{
			var currentUser = "";
			string cookiesName = GetWebConfigParameters("CookiesName");
			HttpCookie authCookie = Request.Cookies[cookiesName];

			if (authCookie == null)
				return currentUser;

			var ticket = FormsAuthentication.Decrypt(authCookie.Value);
			if (ticket == null)
				return currentUser;

			// если тикет устарел
			if (ticket.Expired) {
				// если пользователь хочет зайти в закрытую часть, но доступ устарел 
				if (!IgnoreRoutePermission() && TypeLoginUser != TypeUsers.ControlPanelUser)
					ErrorMessage("Вы достаточно долго не проявляли активность. К сожалению, Ваш сеанс работы завершен. Для продолжения работы вновь авторизуйтесь (введите имя и пароль)");
				return currentUser;
			}

			currentUser = ticket.Name;
			if (!String.IsNullOrEmpty(ticket.UserData))
				CurrentUserIdLog = Convert.ToInt64(ticket.UserData);

			// продлевает сессию, если с пользователем все ок
			SetUserCookiesName(currentUser, true, ticket.UserData);
			return currentUser;
		}

		#region COOKIES
		public void SetUserCookiesName(string UserLoginOrEmail, bool shouldRemember = true, string userData = "")
		{
			var addMinutes = FormsAuthentication.Timeout.TotalMinutes; // 30 мин

			var coockieName = GetWebConfigParameters("CookiesName");
			var ticket = new FormsAuthenticationTicket(
			1,
			UserLoginOrEmail,
			DateTime.Now,
			DateTime.Now.AddMinutes(addMinutes),
			shouldRemember,
			userData,
			FormsAuthentication.FormsCookiePath
			);

			var cookie = new HttpCookie(coockieName, FormsAuthentication.Encrypt(ticket));

			if (shouldRemember) {
				cookie.Expires = DateTime.Now.AddYears(1);
			}

			FormsAuthentication.SetAuthCookie(UserLoginOrEmail, false);
			Response.Cookies.Set(cookie);
		}

		public void SetCookie(string name, string value, bool IsCoding = true)
		{
			var ExpiresDate = DateTime.Now.AddSeconds(15);

			if (name == "AccountName")
			{
				ExpiresDate.AddYears(1);
			}

			if (value == null)
			{
				Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = DateTime.Now });
				return;
			}
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
			var text = "";
			if (IsCoding) { text = Convert.ToBase64String(plainTextBytes); }
			else { text = value; }
			Response.Cookies.Add(new HttpCookie(name, text) { Path = "/" });
		}

		public void DeleteCookie(string name)
		{
			Response.Cookies.Remove(name);
		}

		public void LogOutUser()
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

		/// <summary>
		/// Проверяет, находится ли вызываемый экшн контроллера в списке всегда открытых
		/// </summary>
		/// <returns></returns>
		protected bool IgnoreRoutePermission()
		{
			// список игнорируемых маршрутов CSV. Сейчас Home_Index,FeedBack_*,Account_*
			var ignoreRoute = GetWebConfigParameters("IgnoreRoute").ToLower().Split(',').ToList();
			var result = ignoreRoute.Any(x => x == permissionName || x == (controllerName + "_*").ToLower());
			return result;
		}
	}
}
