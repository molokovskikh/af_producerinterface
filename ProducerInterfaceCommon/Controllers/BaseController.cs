using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;

namespace ProducerInterfaceCommon.Controllers
{
    public class BaseController : GlobalController
    {      
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            CurrentUser = GetCurrentUser(TypeLoginUser);

            if (CurrentUser != null) // присваивается значение текущему пользователю, в наследнике (Так как типов пользователей у нас много)
            {
                CurrentUser.IP = Request.UserHostAddress.ToString();
                ViewBag.CurrentUser = CurrentUser;
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

            bool PermissionExsist = cntx_.ControlPanelPermission.Any(xxx => xxx.Enabled == true && xxx.TypePermission == SbyteTypeUser && xxx.ControllerAction == permissionName && xxx.ActionAttributes == controllerAcctributes);

            if (!PermissionExsist)
            {
                // если пермишена в БД нет, то добавим данный пермишен к группе администраторов

                // проверим наличие группы администраторы

                var AdminGroupName = GetWebConfigParameters("AdminGroupName");

                bool GroupExsist = cntx_.ControlPanelGroup.Any(xxx => xxx.Enabled == true && xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser);

                var GroupAddPermission = new ControlPanelGroup();

                if (!GroupExsist)
                {
                    GroupAddPermission = new ControlPanelGroup { Name = AdminGroupName, Enabled = true, Description = "Администраторы", TypeGroup = SbyteTypeUser };
                    cntx_.ControlPanelGroup.Add(GroupAddPermission);
                }
                else
                {
                    GroupAddPermission = cntx_.ControlPanelGroup.Where(xxx => xxx.Enabled == true && xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser).First();
                }

                var NewPermission = new ControlPanelPermission { ControllerAction = permissionName, ActionAttributes = controllerAcctributes, TypePermission = SbyteTypeUser, Enabled = true, Description = "новый пермишен" };
                cntx_.ControlPanelPermission.Add(NewPermission);
                cntx_.SaveChanges();
                //сохраняем группу и новый пермишен

                GroupAddPermission.ControlPanelPermission.Add(NewPermission);
                // добавляем пермишен к группе
                cntx_.SaveChanges();
            }                    
                // пермишен есть в БД, добавлять ничего не требуется                  
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
            }
            else
            {
                if (!PermissionExsist)
                {
                    // проверяем в БД доступ для текущего пользователя
                    var ListPermission = cntx_.ControlPanelPermission.ToList().Where(xx => xx.ControlPanelGroup.Any(xxx => xxx.ProducerUser.Any(x => x.Id == CurrentUser.Id)))
                        .ToList()
                        .Select(x => new OptionElement { Text = x.ControllerAction + " " + x.ActionAttributes, Value = x.ControllerAction })
                        .ToList();

                    PermissionExsist = ListPermission.Any(xxx => xxx.Text == (permissionName + " " + controllerAcctributes));

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
                return IgnoreRouteForPermission().Any(xxx => xxx == ThisRoute);
            }
            catch { return false; }
        }

        public List<string> IgnoreRouteForPermission()
        {
            // список игнорируемый маршрутов  (хранится в веб конфиге, через запятую в формате Controller_Action)
            try
            {
                return GetWebConfigParameters("IgnoreRoute").ToString().ToLower().Split(new char[] { ',' }).ToList();
            }
            catch { return null; }
        }

        #endregion

        #region /*Возврат залогиненого пользователя из Кукисов (если они существуют)*/

        protected ProducerUser GetCurrentUser(TypeUsers TypeUser_)
        {
            var retUser = new ProducerUser();
            TypeLoginUser = TypeUser_;
            
            if (TypeUser_ == TypeUsers.ProducerUser)
            {
                var EmailUser = GetUserCookiesName();

                if (String.IsNullOrEmpty(EmailUser))
                {
                    return null;
                }

                retUser = cntx_.ProducerUser.Where(xxx => xxx.TypeUser == SbyteTypeUser && xxx.Email == EmailUser && xxx.Enabled == 1).FirstOrDefault();

                if (String.IsNullOrEmpty(retUser.Email))
                {
                    retUser = null;
                }                
            }

            if (TypeUser_ == TypeUsers.ControlPanelUser)
            {
                var LoginUser = GetUserCookiesName();

                if (String.IsNullOrEmpty(LoginUser))
                {
                    return null;
                }

                retUser = cntx_.ProducerUser.Where(xxx => xxx.TypeUser == SbyteTypeUser && xxx.Login == LoginUser && xxx.Enabled == 1).FirstOrDefault();

                if (String.IsNullOrEmpty(retUser.Email))
                {
                    retUser = null;
                }
            }
            return retUser;
        }

        #endregion

        #region /*Аторизация*/
        
        protected void AutorizeCurrentUser(ProducerUser thisUser, TypeUsers TypeUser_)
        {
            var UserLoginName = "";

            if (TypeUser_ == TypeUsers.ProducerUser)
            {
                UserLoginName = thisUser.Email;
                SetUserCookiesName(UserLoginName);

                RedirectToAction("Index", "Profile");
            }

            if (TypeUser_ == TypeUsers.ControlPanelUser)
            {
                UserLoginName = thisUser.Login;
                SetUserCookiesName(UserLoginName);

                RedirectToAction("Index", "Home");
            }
        }
        #endregion

    }
}
