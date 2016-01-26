using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using ProducerInterfaceCommon.ContextModels;
using System.Security.Cryptography;

namespace ProducerInterfaceCommon.Heap
{  
    public partial class BaseUserController : BaseController
    {
        // Объявляем глобальные переменные, для авторизации аутентификации и регистрации пользователей
        
        // Тип пользователя
        protected ContextModels.TypeUsers TypeLoginUser { get { return (ContextModels.TypeUsers)SbyteTypeUser; } set { SbyteTypeUser = (SByte)value; } }
        protected SByte SbyteTypeUser { get; set; }

        // сюда попадёт авторизованный пользователь
        protected ProducerUser CurrentUser { get; set; }

        //DataBase Context
     
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            CurrentUser = GetCurrentUser();
            // получаем текущего пользователя

            var currentActionName = RouteData.Values["action"].ToString().ToLower();
            var currentControllerName = this.GetType().Name.Replace("Controller", "").ToLower();

            var attributesMethod = Request.HttpMethod.ToString().ToLower();


            // проверяем на наличие данного пермишена в игнорируемых
            if (!CheckIgnorePermission((currentControllerName + "_" + currentActionName)))
            {
                // проверка наличия в БД прав для данного экшена
                CheckPermission(currentControllerName, currentActionName, attributesMethod);
            }
            else
            {
                return; // данный экшен контроллера состоит в игнорируемых, можно пользователя далее не мучать
            }


            // пермишен проверен, теперь он есть в БД.

           bool ExsistPermissionUser = CheckUserPermission(CurrentUser, currentControllerName, currentActionName, attributesMethod); // проверяем есть ли данный пермишен у пользователя

            if (!ExsistPermissionUser)
            {
                ErrorMessage("У вас нет доступа к данной странице, или к сзменению данных");
                RedirectToAction("Index", "Home");
            }
                       
        
        }

        public ProducerUser GetCurrentUser()
        {



            return new ProducerUser();

        }

        public bool CheckIgnorePermission(string permissionName)
        {
            try
            {
                var ListIgnore = GetIgnoreRoute();
                return ListIgnore.Any(xxx => xxx == permissionName);
            }
            catch
            {
                return false;
            }
        }
        
        public void LogOut_User()
        {
            // очишаем куки, пользователь более не аутентифицирован
            ClearAllCookies(this.HttpContext);
        }

        public void CheckPermission(string controllerName, string actionName, string attributes = null)
        {
            if (TypeLoginUser == TypeUsers.ProducerUser)
            {
                CheckProducerInterfacePermission(controllerName, actionName, attributes);
            }
            if (TypeLoginUser == TypeUsers.ControlPanelUser)
            {
                CheckControlPanelPermission(controllerName, actionName, attributes);
            }
            if (TypeLoginUser == TypeUsers.ReportUser)
            {
                CheckReportsInterfacePermission(controllerName, actionName, attributes);
            }
            if (TypeLoginUser == TypeUsers.UserRazrab)
            {
                // ничего не делаем
            }

        }

        public bool CheckUserPermission(ProducerUser CurrentUser_, string controllerName, string actionName, string Attributes)
        {

            if (TypeLoginUser == TypeUsers.ProducerUser)
            {
                return CheckProducerUserPermission();
            }
            if (TypeLoginUser == TypeUsers.ControlPanelUser)
            {
                return ChekControlPanelUserPermission();
            }
            if (TypeLoginUser == TypeUsers.ReportUser)
            {
                return ChekReportsInterfaceUserPermission();
            }
            if (TypeLoginUser == TypeUsers.UserRazrab)
            {
                // ничего не делаем
                return true;
            }
            return false;
        }
    }
}
