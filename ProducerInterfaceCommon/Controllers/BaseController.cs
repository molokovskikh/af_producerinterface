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
        }

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

        #region /*Пользователь авторизовался (добавляем кукисы)*/
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
