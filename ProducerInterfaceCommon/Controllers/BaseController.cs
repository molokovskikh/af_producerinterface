using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.Threading;
using System.Web.Caching;

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

			if (CurrentUser != null) // присваивается значение текущему пользователю, в наследнике (Так как типов пользователей у нас много)
			{
				CurrentUser.IP = Request.UserHostAddress.ToString();
				ViewBag.CurrentUser = CurrentUser;
                if (CurrentUserIdLog != CurrentUser.Id)
                {
                    ViewBag.AdminUser = cntx_.Account.Find(CurrentUserIdLog);
                }
            }      
			CheckGlobalPermission(); // проверка наличия пермишена для данного экшена в БД  
			CheckUserPermission(filterContext); // проверка прав у Пользователя к данному сонтроллеру и экшену (Get, Post etc важно для нас)       
		}

		#region /*проверка наличия пермишена в БД или в игнорируемых*/

		public void CheckGlobalPermission()
		{
			//protected string controllerName; - определены ранее
			//protected string actionName; - определены ранее

			if (IgnoreRoutePermission(permissionName))
			{
				return; // найден в игнорируемых
			}
			bool PermissionExsist = cntx_.AccountPermission.Any(xxx => xxx.Enabled == true && xxx.TypePermission == SbyteTypeUser && xxx.ControllerAction == permissionName && xxx.ActionAttributes == controllerAcctributes);

			if (!PermissionExsist)
			{
				// если пермишена в БД нет, то добавим данный пермишен к группе администраторов
				// проверим наличие группы администраторы

				var AdminGroupName = GetWebConfigParameters("AdminGroupName");

				bool GroupExsist = cntx_.AccountGroup.Any(xxx => xxx.Enabled == true && xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser);

				var GroupAddPermission = new AccountGroup();

				if (!GroupExsist)
				{
					GroupAddPermission = new AccountGroup { Name = AdminGroupName, Enabled = true, Description = "Администраторы", TypeGroup = SbyteTypeUser };
					cntx_.AccountGroup.Add(GroupAddPermission);
                    cntx_.SaveChanges();
                }
				else
				{
					GroupAddPermission = cntx_.AccountGroup.Where(xxx => xxx.Enabled == true && xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser).First();
				}

				var NewPermission = new AccountPermission { ControllerAction = permissionName, ActionAttributes = controllerAcctributes, TypePermission = SbyteTypeUser, Enabled = true, Description = "новый пермишен" };
                // добавляем новый доступ
                cntx_.AccountPermission.Add(NewPermission);
				cntx_.SaveChanges();


                // добавляем его к группе Администраторы
                AddPermissionToGroup(GroupAddPermission.Id, NewPermission.Id);            
			}
			// пермишен есть в БД, добавлять ничего не требуется                  
		}
        
        private void AddPermissionToGroup(int GroupId, int PermissionId)
        {
            var GroupItem = cntx_.AccountGroup.Find(GroupId);
            var PermissionItem = cntx_.AccountPermission.Find(PermissionId);
            GroupItem.AccountPermission.Add(PermissionItem);
            cntx_.Entry(GroupItem).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

         
        }
           

		#endregion

		#region /*Проверка прав пользователя*/

		public void CheckUserPermission(ActionExecutingContext filterContext)
		{
			var PermissionExsist = false;
			// проверяем список игнорируемых маршрутов
			PermissionExsist = IgnoreRoutePermission(permissionName);

			if (CurrentUser == null)
			{
				if (TypeLoginUser == TypeUsers.ProducerUser)
				{
					if (!PermissionExsist)
					{
						filterContext.Result = RedirectToAction("Index", "Home");
					}
				}
				if (TypeLoginUser == TypeUsers.ControlPanelUser)
				{
					if (!PermissionExsist)
					{
						filterContext.Result = RedirectToAction("Index", "Registration");
					}
				}
				if (TypeLoginUser == TypeUsers.UserNotProducer)
				{
					if (!PermissionExsist)
					{
						filterContext.Result = RedirectToAction("Index", "Home");
					}
				}
			}
			else
			{
				if (!PermissionExsist)
				{
					// проверяем в БД доступ для текущего пользователя         

					PermissionExsist = PermissionUserExsist();

					if (!PermissionExsist) // если нет доступа, редиректим на стартовую страницу
					{
						ErrorMessage("У вас нет прав доступа к запрашиваемой странице. Или для изменения данных");
						filterContext.Result = RedirectToAction("Index", "Home");
					}
				}
				else
				{
					//   ErrorMessage("У вас нет прав доступа к запрашиваемой странице. Или для изменения данных");
					//   filterContext.Result = RedirectToAction("Index", "Registration");
				}
			}

		}

		private bool IgnoreRoutePermission(string ThisRoute)
		{          
			try
			{


                List<string> IgnoreRoute = IgnoreRouteForPermission();

                foreach (var ItemIgnore in IgnoreRoute)
                {
                    if (ItemIgnore == ThisRoute || ItemIgnore.ToLower() == (controllerName + "_*").ToLower())
                    {                      
                        return true;
                    }
                }    
			}
			catch { return false; }

            return false;
		}

		public List<string> IgnoreRouteForPermission()
		{
			// список игнорируемый маршрутов  (хранится в веб конфиге, через запятую в формате Controller_Action && Controller_* где * игнорируются все акшены в данном контроллере)
			try
			{
				return GetWebConfigParameters("IgnoreRoute").ToString().ToLower().Split(new char[] { ',' }).ToList();
			}
			catch { return null; }
		}

		#endregion

		public bool PermissionUserExsist()
		{
			var permissionList = GetPermissionList();
			var permissionKey = $"{permissionName}_{controllerAcctributes}";
			return permissionList.Contains(permissionKey);
		}

		public HashSet<string> GetPermissionList()
		{
			var key = $"permission{CurrentUser.Id}";
			var permissionList = HttpContext.Cache.Get(key) as HashSet<string>;
			if (permissionList != null)
            {
                var lastUpdatePermissiion = GetLastDate();
                if (lastUpdatePermissiion.First().ToLower() == permissionList.First().ToLower())
                {
                    return permissionList;
                }
            }
            permissionList = new HashSet<string>();

            if (CurrentUser.LastUpdatePermisison == null)
            {
                permissionList.Add(DateTime.MinValue.Year.ToString());              
            }
            else
            {
                permissionList.Add(CurrentUser.LastUpdatePermisison.ToString());              
            }

            permissionList.Add(CurrentUser.LastUpdatePermisison.ToString());

            foreach (var group in CurrentUser.AccountGroup)
            {
                foreach (var permission in group.AccountPermission)
                {
                    var permissionKey = $"{permission.ControllerAction}_{permission.ActionAttributes}";
                    if (permission.Enabled && !permissionList.Contains(permissionKey))
                        permissionList.Add(permissionKey);
                }
            }

			HttpContext.Cache.Insert(key, permissionList, null, DateTime.UtcNow.AddSeconds(10), Cache.NoSlidingExpiration);
			return permissionList;
		}

        public HashSet<string> GetLastDate()
        {
            var key = $"date{CurrentUser.Id}";
            var DateList = HttpContext.Cache.Get(key) as HashSet<string>;

            if (DateList != null) { return DateList; }

            DateList = new HashSet<string>();

            if (CurrentUser.LastUpdatePermisison == null)
            {
                DateList.Add(DateTime.MinValue.Year.ToString());
            }
            else
            {
                DateList.Add(CurrentUser.LastUpdatePermisison.ToString());
            }
          
            HttpContext.Cache.Insert(key, DateList, null, DateTime.UtcNow.AddSeconds(1), Cache.NoSlidingExpiration);
            return DateList;
        }       
         

        #region /*Возврат залогиненого пользователя из Кукисов (если они существуют)*/

        protected Account GetCurrentUser(TypeUsers TypeUser_)
		{
			var retUser = new Account();
			TypeLoginUser = TypeUser_;

			if (TypeUser_ == TypeUsers.ProducerUser)
			{
				var EmailUser = GetUserCookiesName();

				if (String.IsNullOrEmpty(EmailUser))
				{
					return null;
				}

				retUser = cntx_.Account.Where(xxx => xxx.TypeUser == SbyteTypeUser && xxx.Login == EmailUser && xxx.Enabled == 1).FirstOrDefault();

                if (retUser == null || String.IsNullOrEmpty(retUser.Login))
                {
                    retUser = null;
                }
                else
                {
                    if (CurrentUserIdLog > 0)
                    {
                        retUser.ID_LOG = CurrentUserIdLog;
                    }
                    else
                    {
                        retUser.ID_LOG = retUser.Id;
                    }
                }
			}

			if (TypeUser_ == TypeUsers.ControlPanelUser)
			{
				var LoginUser = GetUserCookiesName();

				if (String.IsNullOrEmpty(LoginUser))
				{
					return null;
				}

				retUser = cntx_.Account.Where(xxx => xxx.TypeUser == SbyteTypeUser && xxx.Login == LoginUser && xxx.Enabled == 1).First();

				if (String.IsNullOrEmpty(retUser.Login))
				{
					retUser = null;
				}
			}

			if (TypeUser_ == TypeUsers.UserNotProducer)
			{
				var EmailUser = GetUserCookiesName();

				if (String.IsNullOrEmpty(EmailUser))
				{
					return null;
				}

				retUser = cntx_.Account.Where(xxx => xxx.TypeUser == SbyteTypeUser && xxx.Login == EmailUser && xxx.Enabled == 1).FirstOrDefault();

				if (String.IsNullOrEmpty(retUser.Login))
				{
					retUser = null;
				}
			}
			return retUser;
		}

		#endregion

		#region /*Аторизация*/

		protected void AutorizeCurrentUser(Account thisUser, TypeUsers TypeUser_)
		{
			var UserLoginName = "";

			if (TypeUser_ == TypeUsers.ProducerUser)
			{
				UserLoginName = thisUser.Login;
				SetUserCookiesName(UserLoginName);

				RedirectToAction("Index", "Profile");
			}

			if (TypeUser_ == TypeUsers.ControlPanelUser)
			{
				UserLoginName = thisUser.Login;
				SetUserCookiesName(UserLoginName);

				RedirectToAction("Index", "Home");
			}
			if (TypeUser_ == TypeUsers.UserNotProducer)
			{
				UserLoginName = thisUser.Login;
				SetUserCookiesName(UserLoginName);

				RedirectToAction("Index", "Home");
			}

		}
		#endregion

	}
}
