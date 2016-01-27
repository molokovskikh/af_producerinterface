using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;


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
                ViewBag.CurrentUser = CurrentUser;
            }

            CheckGlobalPermission();
            CheckUserPermission();
        }

        #region /*проверка наличия пермишена в БД или в игнорируемых*/

        public void CheckGlobalPermission()
        {
            if (TypeLoginUser == TypeUsers.ProducerUser)
            {
                // пермишены производителей

                // PermissionType = 0

                throw new Exception();




            }
            if (TypeLoginUser == TypeUsers.ControlPanelUser)
            {
                // пермишены панели управления

                // PermissionType = 1


                throw new Exception();


            }

        }

        #endregion

        #region /*Проверка прав пользователя*/

        public void CheckUserPermission()
        {
            if (TypeLoginUser == TypeUsers.ProducerUser)
            {
                // пермишены производителей

                // PermissionType = 0

                throw new Exception();




            }
            if (TypeLoginUser == TypeUsers.ControlPanelUser)
            {
                // пермишены панели управления

                // PermissionType = 1


                throw new Exception();


            }
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
