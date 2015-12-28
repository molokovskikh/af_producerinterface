using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;
using System.Configuration;
using System.ComponentModel;
using System.Data.Entity;

namespace ProducerInterface.Controllers
{
    public class BaseProducerInterfaceController : BaseController
    {

        //OnActionExecuting
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Инициализация пользователя Cockie
            // Проверка доступа к акшену           
            base.OnActionExecuting(filterContext);

            string[] ignorePermissionRedirect = ConfigurationManager.AppSettings["IgnoreAction"].ToString().ToLower().Split(new char[] { ',' });

            var _User = GetCurrentUser();
            ViewBag.CurrentUser = _User;

            ViewBag.LoginModel = new Models.LoginValidation();


            CheckForExsistenceOfUserPermission(this);
            AuthentificationModule.CheckPermission(filterContext, _User, ignorePermissionRedirect);        
        }

        
      
        public produceruser GetCurrentUser(bool getFormSession = true)
        {
            if (CurrentAnalitUser == null || (CurrentAnalitUser.Name == String.Empty))
            {
                CurrentProduserUser = null;
                return null;
            }
            if (getFormSession && (CurrentProduserUser == null || CurrentProduserUser.Name != CurrentProduserUser.Email))
            {
              //  string Name = 
                CurrentProduserUser = DB.produceruser.FirstOrDefault(e => e.Email == CurrentAnalitUser.Name);
            }
            return CurrentProduserUser;
        }

        // проверка наличия пермишена для контроллера, 1. есть ли он в Ignore Листе (список хранится в Web.config key=IgnoreAction / через запятую)
        public static void CheckForExsistenceOfUserPermission(Controller CurentController)
        {
            bool NotFoundPermission = true; // поумолчанию true -- пока не найден в списке игнорируемых или в списке БД.

            // получаем список игнорируемых акшенов и webconfig > IgnoreAction  (список игнорируемых экшенов перечислен через запятую.)
            string[] IgnoreAction = ConfigurationManager.AppSettings["IgnoreAction"].ToString().ToLower().Split(new char[] { ',' });
                     
            string controllerName = CurentController.GetType().Name.Replace("Controller", "").ToLower().ToString();
            string actionName = CurentController.Request.RequestContext.RouteData.GetRequiredString("action").ToLower().ToString();

            string PermissionName = controllerName + "_" + actionName;

            foreach (var PermStrWebConfig in IgnoreAction)
            {
                if (PermStrWebConfig == PermissionName)
                {
                    NotFoundPermission = false;
                    break;
                }
            }

            if (NotFoundPermission)
            {
          
                var listPermission = DB.userpermission
                    .Where(xxx => xxx.Name == PermissionName)
                    .Select(xx =>xx.Name.ToLower()).ToList();

                // проверяем наличии доступа в БД. // таблица содержит список контролеров + акшенов // на момент разработки не более 25. (запрос пыполнится моментально)

                if (listPermission == null || listPermission.Count() == 0)
                {
                    // если в БД не найден пермишн, намм надо его добавить

                    var permishion = new userpermission();
                    var url = CurentController.Url.Action(actionName, controllerName);
                    permishion.Description = "Доступ к странице <a href='" + url + "'>" + PermissionName + "</a>";
                    permishion.Name = PermissionName;

                    DB.Entry(permishion).State = System.Data.Entity.EntityState.Added;
                    DB.SaveChanges();
                    //  и дать доступ всем пользователям (которые первыми зарегистрировались и подтвердили регистрацию от каждого Производителя.)

                    List<produceruser> UsersProducerUser = DB.produceruser.Where(xxx => xxx.Enabled == 1)
                        .GroupBy(xxx => xxx.ProducerId).Select(xxx => xxx.OrderByDescending(p => p.ProducerId).FirstOrDefault()).ToList();

                    long idPermission = DB.userpermission.Where(xxx => xxx.Name == PermissionName).First().Id;


                    foreach (var User in UsersProducerUser)
                    {
                        long IdUser = User.Id;
                        usertouserrole NewRole = new usertouserrole();
                        NewRole.UserPermissionId = idPermission;
                        NewRole.ProducerUserId = User.Id;
                        DB.Entry(NewRole).State = System.Data.Entity.EntityState.Added;
                    }

                    DB.SaveChanges();
                }
            }            
        }
    }
}