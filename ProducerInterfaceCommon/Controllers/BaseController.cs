using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Caching;
using log4net;

namespace ProducerInterfaceCommon.Controllers
{
	public class BaseController : Controller
	{
		protected producerinterface_Entities DB = new producerinterface_Entities();
		protected Context DB2 = new Context();
		protected string controllerName;
		protected string actionName;
		protected string controllerAcctributes;
		protected string permissionName => (controllerName + "_" + actionName).ToLower();

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			controllerName = GetType().Name.Replace("Controller", "").ToLower();
			actionName = Request.RequestContext.RouteData.GetRequiredString("action").ToLower();
			controllerAcctributes = Request.HttpMethod.ToLower();

			ViewBag.UrlString = HttpContext.Request.Url;
			ViewBag.Env = ConfigurationManager.AppSettings["env"];
			ThreadContext.Properties["url"] = HttpContext.Request.Url;
			ThreadContext.Properties["user"] = HttpContext.User.Identity.Name;
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

		public string GetWebConfigParameters(string key)
		{
			return System.Configuration.ConfigurationManager.AppSettings[key];
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

		public string GetRandomPassword()
		{
			return Guid.NewGuid().ToString().Replace("-", "").ToLower().Substring(8, 6);
		}

		protected void SecurityCheck(Account user, TypeUsers accessType, ActionExecutingContext filterContext)
		{
			// если найден в игнорируемых
			if (IgnoreRoutePermission())
				return;

			// проверка наличия пермишена в БД и добавление в случае отсутствия
			AddPermission(accessType);

			// проверка прав у Пользователя к данному сонтроллеру и экшену (Get, Post etc важно для нас)
			CheckUserPermission(user, accessType, filterContext);
		}

		/// <summary>
		/// проверка наличия пермишена в БД и добавление в случае отсутствия
		/// </summary>
		private void AddPermission(TypeUsers accessType)
		{
			var type = (sbyte)accessType;
			var permissionExsist = DB.AccountPermission.Any(x =>
				x.TypePermission == type
				&& x.ControllerAction == permissionName
				&& x.ActionAttributes == controllerAcctributes);

			// пермишен есть в БД, добавлять ничего не требуется
			if (permissionExsist)
				return;

			// если пермишена в БД нет, то добаляем пермишен к группе администраторов

			// проверим наличие группы "Администраторы"
			var adminGroupName = GetWebConfigParameters("AdminGroupName");
			// убрал условие Enabled потому что группа будет создана заново, если disabled
			var adminGroup = DB.AccountGroup.SingleOrDefault(x => x.Name == adminGroupName && x.TypeGroup == type);
			if (adminGroup == null)
			{
				adminGroup = new AccountGroup { Name = adminGroupName, Enabled = true, Description = "Администраторы", TypeGroup = type };
				DB.AccountGroup.Add(adminGroup);
				DB.SaveChanges();
			}

			// добавляем новый доступ
			var newPermission = new AccountPermission { ControllerAction = permissionName, ActionAttributes = controllerAcctributes, TypePermission = type, Enabled = true, Description = "новый пермишен" };
			DB.AccountPermission.Add(newPermission);
			DB.SaveChanges();

			// добавляем его к группе Администраторы
			adminGroup.AccountPermission.Add(newPermission);

			var date = DateTime.Now;
			foreach (var user in adminGroup.Account)
				user.LastUpdatePermisison = date;

			DB.SaveChanges();
		}

		/// <summary>
		/// Проверка прав пользователя
		/// </summary>
		/// <param name="filterContext"></param>
		private void CheckUserPermission(Account user, TypeUsers accessType, ActionExecutingContext filterContext)
		{
			// если пользователя нет, но он пришёл из панели управления - на регистрацию
			if (user == null && accessType == TypeUsers.ControlPanelUser)
				filterContext.Result = RedirectToAction("Index", "Registration");

			// если пользователя нет - на главную
			else if (user == null)
				filterContext.Result = Redirect("~");

			// если есть пользователь и права доступа
			else if (PermissionUserExsist(user, accessType))
				return;

			// если есть пользователь и нет прав доступа
			else {
				ErrorMessage("У вас нет прав доступа к запрашиваемой странице");
				var refferer = filterContext.HttpContext.Request.UrlReferrer;
				if (refferer != null && !String.IsNullOrEmpty(refferer.OriginalString))
					filterContext.Result = Redirect(refferer.OriginalString);
				else
					filterContext.Result = Redirect("~");
			}
		}

		/// <summary>
		/// Проверяет наличие пермишена в списке пермишенов пользователя
		/// </summary>
		/// <returns></returns>
		public bool PermissionUserExsist(Account user, TypeUsers accessType)
		{
			var permissionList = GetPermissionList(user, accessType);
			var permissionKey = $"{permissionName}_{controllerAcctributes}";
			return permissionList.Contains(permissionKey);
		}

		/// <summary>
		/// Возврашает список пермишенов пользователя
		/// </summary>
		/// <returns></returns>
		public HashSet<string> GetPermissionList(Account user, TypeUsers accessType)
		{
			var type = (sbyte)accessType;
			var key = $"permission{user.Id}";
			var permissionList = HttpContext.Cache.Get(key) as HashSet<string>;
			// если есть в кеше - возвращаем из кеша
			if (permissionList != null) {
				// в первой строке лежит время создания кеша
				var dateHash = DateTime.Parse(permissionList.First());
				// если права пользователя не менялись с момента создания кеша
				if (user.LastUpdatePermisison.HasValue && dateHash > user.LastUpdatePermisison.Value)
					return permissionList;
			}
			permissionList = new HashSet<string>();

			// добавляем время обновления кеша первой строкой
			permissionList.Add(DateTime.Now.ToString("O"));

			var ps = user.AccountGroup.SelectMany(x => x.AccountPermission)
				.Where(x => x.Enabled && x.TypePermission == type)
				.Select(x => $"{x.ControllerAction}_{x.ActionAttributes}")
				.Distinct().ToList();

			foreach (var p in ps)
				permissionList.Add(p);

			HttpContext.Cache.Insert(key, permissionList, null, DateTime.UtcNow.AddSeconds(300), Cache.NoSlidingExpiration);
			return permissionList;
		}
	}
}
