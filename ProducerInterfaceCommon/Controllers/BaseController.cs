using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.Web.Caching;
using log4net;

namespace ProducerInterfaceCommon.Controllers
{
	public class BaseController : GlobalController
	{
		protected override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			base.OnActionExecuting(filterContext);
			CurrentUser = GetCurrentUser(TypeLoginUser);

			ViewBag.UrlString = HttpContext.Request.Url;
			ThreadContext.Properties["url"] = HttpContext.Request.Url;

			// присваивается значение текущему пользователю, в наследнике (Так как типов пользователей у нас много)
			if (CurrentUser != null) 
			{
				ThreadContext.Properties["user"] = CurrentUser.Name ?? CurrentUser.Login;
				CurrentUser.IP = Request.UserHostAddress;
				ViewBag.CurrentUser = CurrentUser;
				if (CurrentUserIdLog != CurrentUser.Id)
					ViewBag.AdminUser = cntx_.Account.Find(CurrentUserIdLog);
			}
			// если найден в игнорируемых
			if (IgnoreRoutePermission())
				return;

			// проверка наличия пермишена в БД и добавление в случае отсутствия
			AddPermission();
			
			// проверка прав у Пользователя к данному сонтроллеру и экшену (Get, Post etc важно для нас)
			CheckUserPermission(filterContext);        
		}

		/// <summary>
		/// проверка наличия пермишена в БД и добавление в случае отсутствия
		/// </summary>
		private void AddPermission()
		{
			var permissionExsist = cntx_.AccountPermission.Any(x => 
				x.TypePermission == SbyteTypeUser 
				&& x.ControllerAction == permissionName 
				&& x.ActionAttributes == controllerAcctributes);

			// пермишен есть в БД, добавлять ничего не требуется                  
			if (permissionExsist)
				return;

			// если пермишена в БД нет, то добаляем пермишен к группе администраторов

			// проверим наличие группы "Администраторы"
			var adminGroupName = GetWebConfigParameters("AdminGroupName");
			// убрал условие Enabled потому что группа будет создана заново, если disabled
			var adminGroup = cntx_.AccountGroup.SingleOrDefault(x => x.Name == adminGroupName && x.TypeGroup == SbyteTypeUser);
			if (adminGroup == null)
			{
				adminGroup = new AccountGroup { Name = adminGroupName, Enabled = true, Description = "Администраторы", TypeGroup = SbyteTypeUser };
				cntx_.AccountGroup.Add(adminGroup);
				cntx_.SaveChanges();
			}

			// добавляем новый доступ
			var newPermission = new AccountPermission { ControllerAction = permissionName, ActionAttributes = controllerAcctributes, TypePermission = SbyteTypeUser, Enabled = true, Description = "новый пермишен" };
			cntx_.AccountPermission.Add(newPermission);
			cntx_.SaveChanges();

			// добавляем его к группе Администраторы
			adminGroup.AccountPermission.Add(newPermission);
			//cntx_.Entry(adminGroup).State = System.Data.Entity.EntityState.Modified;

			var date = DateTime.Now;
			foreach (var user in adminGroup.Account)
				user.LastUpdatePermisison = date;

			cntx_.SaveChanges();
		}

		/// <summary>
		/// Проверка прав пользователя
		/// </summary>
		/// <param name="filterContext"></param>
		private void CheckUserPermission(ActionExecutingContext filterContext)
		{
			// если пользователя нет, но он пришёл из панели управления - на регистрацию
			if (CurrentUser == null && TypeLoginUser == TypeUsers.ControlPanelUser)
				filterContext.Result = RedirectToAction("Index", "Registration");

			// если пользователя нет - на главную
			else if (CurrentUser == null)
				filterContext.Result = RedirectToAction("Index", "Home");

			// если есть пользователь и права доступа
			else if (PermissionUserExsist())
				return;

			// если есть пользователь и нет прав доступа
			else {
				ErrorMessage("У вас нет прав доступа к запрашиваемой странице");
				var refferer = filterContext.HttpContext.Request.UrlReferrer;
				if (refferer != null && !String.IsNullOrEmpty(refferer.OriginalString))
					filterContext.Result = Redirect(refferer.OriginalString);
				else
					filterContext.Result = RedirectToAction("Index", "Home");
			}
		}

		/// <summary>
		/// Проверяет, находится ли вызываемый экшн контроллера в списке всегда открытых
		/// </summary>
		/// <returns></returns>
		private bool IgnoreRoutePermission()
		{
			// список игнорируемых маршрутов CSV. Сейчас Home_Index,FeedBack_*,Account_*
			var ignoreRoute = GetWebConfigParameters("IgnoreRoute").ToLower().Split(',').ToList();
			var result = ignoreRoute.Any(x => x == permissionName || x == (controllerName + "_*").ToLower());
			return result;
		}

		/// <summary>
		/// Проверяет наличие пермишена в списке пермишенов пользователя
		/// </summary>
		/// <returns></returns>
		public bool PermissionUserExsist()
		{
			var permissionList = GetPermissionList();
			var permissionKey = $"{permissionName}_{controllerAcctributes}";
			return permissionList.Contains(permissionKey);
		}

		/// <summary>
		/// Возврашает список пермишенов пользователя
		/// </summary>
		/// <returns></returns>
		public HashSet<string> GetPermissionList()
		{
			var key = $"permission{CurrentUser.Id}";
			var permissionList = HttpContext.Cache.Get(key) as HashSet<string>;
			// если есть в кеше - возвращаем из кеша
			if (permissionList != null) {
				// в первой строке лежит время создания кеша
				var dateHash = DateTime.Parse(permissionList.First());
				// если права пользователя не менялись с момента создания кеша
				if (CurrentUser.LastUpdatePermisison.HasValue && dateHash > CurrentUser.LastUpdatePermisison.Value)
					return permissionList;
			}
			permissionList = new HashSet<string>();

			// добавляем время обновления кеша первой строкой
			permissionList.Add(DateTime.Now.ToString("O"));

			var ps = CurrentUser.AccountGroup.SelectMany(x => x.AccountPermission)
				.Where(x => x.Enabled && x.TypePermission == SbyteTypeUser)
				.Select(x => $"{x.ControllerAction}_{x.ActionAttributes}")
				.Distinct().ToList();

			foreach (var p in ps)
				permissionList.Add(p);

			HttpContext.Cache.Insert(key, permissionList, null, DateTime.UtcNow.AddSeconds(300), Cache.NoSlidingExpiration);
			return permissionList;
		}

		/// <summary>
		/// Возвращает текущего пользователя из Кукисов (если они существуют)
		/// </summary>
		/// <param name="typeUser">enum Тип пользователя</param>
		/// <returns></returns>
		protected Account GetCurrentUser(TypeUsers typeUser)
		{
			TypeLoginUser = typeUser;
			var login = GetUserCookiesName();
			if (String.IsNullOrEmpty(login))
				return null;

			// TODO: First - это неправильно
			var retUser = cntx_.Account.FirstOrDefault(x => x.TypeUser == SbyteTypeUser && x.Login == login && x.Enabled == 1);
			if (retUser != null && typeUser == TypeUsers.ProducerUser)
			{
				retUser.ID_LOG = retUser.Id;
				if (CurrentUserIdLog > 0)
					retUser.ID_LOG = CurrentUserIdLog;
			}
			return retUser;
		}

		///// <summary>
		///// Аторизация
		///// </summary>
		///// <param name="user">Текущий пользователь</param>
		///// <param name="typeUser">Тип пользователя</param>
		///// <param name="userData">Некие данные пользователя</param>
		//protected void AutorizeCurrentUser(Account user, TypeUsers typeUser, string userData = null)
 	//	{
		//	if (typeUser == TypeUsers.ProducerUser) {
		//		SetUserCookiesName(user.Login, true, userData);
		//		SetCookie("AccountName", user.Login, false);
 	//			RedirectToAction("Index", "Profile");
 	//		}
		//	else {
		//		SetUserCookiesName(user.Login);
		//		RedirectToAction("Index", "Home");
		//	}
 	//	}
	}
}
