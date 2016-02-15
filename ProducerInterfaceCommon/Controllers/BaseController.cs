using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;

namespace ProducerInterfaceCommon.Controllers
{
    public class BaseController : GlobalController
    {

        System.Diagnostics.Stopwatch STW = new System.Diagnostics.Stopwatch();

        private List<STP> STW_List = new List<STP>();

        private class STP
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            STW.Stop();
            STW_List.Add( new STP {Key= "Скорость отработки Экшена",Value= STW.ElapsedMilliseconds.ToString() });
        }

        private void SaveTimerToDatabase(List<STP> Save)
        {
            var 

        }


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            STW.Start();
            base.OnActionExecuting(filterContext);
            STW.Stop();

            STW_List.Add(new STP { Key = "Base Executing", Value = STW.ElapsedMilliseconds.ToString() });

            STW.Start();
            CurrentUser = GetCurrentUser(TypeLoginUser);
            STW.Stop();
            STW_List.Add(new STP { Key = "Получение модели пользователя из кукисов", Value = STW.ElapsedMilliseconds.ToString() });


            if (CurrentUser != null) // присваивается значение текущему пользователю, в наследнике (Так как типов пользователей у нас много)
            {
                CurrentUser.IP = Request.UserHostAddress.ToString();
                ViewBag.CurrentUser = CurrentUser;
            }

            STW.Start();
            CheckGlobalPermission(); // проверка наличия пермишена для данного экшена в БД
            STW.Stop();
            STW_List.Add(new STP { Key = "Проверка наличия доступа", Value = STW.ElapsedMilliseconds.ToString() });

         //   STW.Start();
            CheckUserPermission(filterContext); // проверка прав у Пользователя к данному сонтроллеру и экшену (Get, Post etc важно для нас)
         //   STW.Stop();
         //   STW_List.Add(new STP { Key = "Проверка прав у пользователя", Value = STW.ElapsedMilliseconds.ToString() });
         //   STW.Start();
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
                }
                else
                {
                    GroupAddPermission = cntx_.AccountGroup.Where(xxx => xxx.Enabled == true && xxx.Name == AdminGroupName && xxx.TypeGroup == SbyteTypeUser).First();
                }

                var NewPermission = new AccountPermission { ControllerAction = permissionName, ActionAttributes = controllerAcctributes, TypePermission = SbyteTypeUser, Enabled = true, Description = "новый пермишен" };
                cntx_.AccountPermission.Add(NewPermission);
                cntx_.SaveChanges();
                //сохраняем группу и новый пермишен

                GroupAddPermission.AccountPermission.Add(NewPermission);
                cntx_.Entry(GroupAddPermission).State = System.Data.Entity.EntityState.Modified;
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


            STW.Start();
            PermissionExsist = IgnoreRoutePermission(permissionName);
            STW.Stop();
            STW_List.Add(new STP { Key = "Проверка списка игноррируемых", Value = STW.ElapsedMilliseconds.ToString() });
            
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

                    STW.Start();            
                    var ListPermission = cntx_.AccountPermission.Where(xxx=>xxx.TypePermission == SbyteTypeUser).ToList().Where(xx => xx.AccountGroup.Any(xxx => xxx.Account.Any(x => x.Id == CurrentUser.Id)))
                        .ToList();

                    PermissionExsist = ListPermission.Any(xxx => xxx.ControllerAction == permissionName && xxx.ActionAttributes == controllerAcctributes);

                    STW.Stop();
                    STW_List.Add(new STP { Key = "Проверка в БД на наличие доступа", Value = STW.ElapsedMilliseconds.ToString() });

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

            STW.Start();
        }

        private bool IgnoreRoutePermission(string ThisRoute)
        {


            try
            {
                //return true;
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

        public bool PermissionUserExsist()
        {
            

            return false;
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
            }

            if (TypeUser_ == TypeUsers.ControlPanelUser)
            {
                var LoginUser = GetUserCookiesName();

                if (String.IsNullOrEmpty(LoginUser))
                {
                    return null;
                }

                retUser = cntx_.Account.Where(xxx => xxx.TypeUser == SbyteTypeUser && xxx.Login == LoginUser && xxx.Enabled == 1).FirstOrDefault();

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
