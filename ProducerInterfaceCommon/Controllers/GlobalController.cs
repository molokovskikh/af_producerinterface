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

		public void SetCookie(string name, string value, bool IsCoding = true)
		{
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
			return System.Configuration.ConfigurationManager.AppSettings[ParamKey];
		}

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
