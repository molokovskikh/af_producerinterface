using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;
using System.Web.Security;

namespace ProducerInterface.Controllers.pruducercontroller
{
    public class BaseController : ConfigurationController
    {  
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
             base.OnActionExecuting(filterContext);

            // действие до начала нашего экшена      

            AutorizedUser = GetUser(this);

            CurrentUser = GetCurrentUser();            
            ViewBag.CurrentUser = CurrentUser;

            //// проверяем есть ли пермишн к данному контролер/акшену,
            ////если нет то он добавится в БД и будет дан доступ всем Первым зарегистрировавшимся пользователям от каждого Производителя
            CheckForExsistenceOfUserPermission();           

            //// проверяем права у пользователя 
            CheckPermission(filterContext, CurrentUser);
        }
        
        public produceruser GetUser(Controller currentController)
        {
            var currentUser = new produceruser();

            string cookiesName = GetCoockieName;

            try
            {
                HttpCookie authCookie = Request.Cookies[cookiesName];
                if (authCookie != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    string cookiePath = ticket.CookiePath;
                    DateTime expiration = ticket.Expiration;
                    bool expired = ticket.Expired;
                    bool isPersistent = ticket.IsPersistent;
                    DateTime issueDate = ticket.IssueDate;
                    string name = ticket.Name;
                    string userData = ticket.UserData;
                    string version = ticket.Version.ToString();
                    currentUser.Name = name;             
                    authCookie.Expires = SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
                    Response.Cookies.Set(authCookie);
                }
            }
            catch (Exception) { /*IGNORE*/ }

            return currentUser;
        }

        public void CheckPermission(ActionExecutingContext filterContext, produceruser user_)
        {
            var actionName = filterContext.RouteData.Values["action"].ToString();
            var controllerName = filterContext.Controller.GetType().Name.Replace("Controller", "");
            string currentPermissionName = controllerName.ToLower() + "_" + actionName.ToLower();

            if (!IgnoreRoutePermission(currentPermissionName)) // проверка маршрута на наличие в игнорируемых
            {
                if (CurrentUser == null)
                {
                    // CurrentUser = null если не авторизован
                    string url = GetunAuthorizedUserRedirectUrl;
                    string actionName_ = (url.Split(new Char[] { '/' }))[1];
                    string controllerName_ = (url.Split(new Char[] { '/' }))[0];
                    if (url != String.Empty)
                    {
                        ClearAllCookies();
                        this.ErrorMessage("У вас нет прав доступа к запрашиваемой странице.");
                        filterContext.Result = RedirectToAction(actionName_.ToLower(), controllerName_.ToLower());
                        // filterContext.Result = new RedirectResult(url);
                    }
                }
                else
                {                   

                    var PermissionsUser = _BD_.userpermission.FirstOrDefault(x => x.Name.ToLower() == currentPermissionName);
                    
                    if (CurrentUser != null && currentPermissionName != null && !CheckPermission(PermissionsUser))
                    {
                        //string url = GetredirectAfterAuthentication;
                        //string actionName_ = (url.Split(new Char[] { '/' }))[1];
                        //string controllerName_ = (url.Split(new Char[] { '/' }))[0];
                        //filterContext.Result = RedirectToAction(actionName_, controllerName_);
                        // доступ есть!
                        string url = GetredirectAfterAuthentication;


                    }
                    else
                    {
                        this.ErrorMessage("У вас нет прав доступа к запрашиваемой странице.");
                        string url = GetredirectAfterAuthentication;
                        string actionName_ = (url.Split(new Char[] { '/' }))[1];
                        string controllerName_ = (url.Split(new Char[] { '/' }))[0];
                        if (url != String.Empty)
                        {
                            filterContext.Result = RedirectToAction(actionName_.ToLower(), controllerName_.ToLower());
                        }

                    }
                }
            }
        }

        public bool CheckPermission(userpermission permission)
        {
            if (permission == null)
            {
                return false;
            }

           bool X = GetUserPermissions().Any(s => s.userpermission.Name.ToLower() == permission.Name.ToLower());
           return X;
        }

        public IList<usertouserrole> GetUserPermissions()
        {            
            var permissionsList = AutorizedUser.usertouserrole.ToList();          
            return permissionsList;
        }

        public string IsSecuredController(Controller currentController, ActionExecutingContext filterContext, bool redirectAnyway = false)
        {
            var isToBeSecured = this.GetType().GetCustomAttributes(typeof(SecuredControllerAttribute), true).Any();
            var currentActionName = RouteData.Values["action"].ToString().ToLower();
            var currentControllerName = this.GetType().Name.Replace("Controller", "").ToLower();

            var unAuthorizedUserRedirectUrl = GetunAuthorizedUserRedirectUrl;
            string[] urlUnAuthorizedUser = unAuthorizedUserRedirectUrl.Split('/');
            var controllerUnAuthorizedUser = urlUnAuthorizedUser[0].ToLower();
            var actionUnAuthorizedUser = urlUnAuthorizedUser.Length > 1 ? urlUnAuthorizedUser[1].ToLower() : "index";

            var RedirectAfterAuthenticationUrl = GetredirectAfterAuthentication;
            string[] urlAfterAuthentication = RedirectAfterAuthenticationUrl.Split('/');
            var controllerAfterAuthentication = urlAfterAuthentication[0].ToLower();
            var actionAfterAuthentication = urlAfterAuthentication.Length > 1 ? urlAfterAuthentication[1].ToLower() : "index";

            var currentUserName = redirectAnyway
                ? ""
                : ((Controller)this != null
                && this.AutorizedUser != null
                ? AutorizedUser.Name : ""
                );
            if (isToBeSecured && string.IsNullOrEmpty(currentUserName)
                && (currentActionName != actionUnAuthorizedUser
                || (currentActionName != "" && actionUnAuthorizedUser == "Index")
                && currentControllerName != controllerUnAuthorizedUser))
            {
                return currentController.Url.Action(actionUnAuthorizedUser, controllerUnAuthorizedUser);
            }
            if (!string.IsNullOrEmpty(currentUserName) && currentActionName == actionUnAuthorizedUser &&
               currentControllerName == controllerUnAuthorizedUser)
            {
                return currentController.Url.Action(actionAfterAuthentication, controllerAfterAuthentication);
            }

            return currentController.Url.Action(actionUnAuthorizedUser, controllerUnAuthorizedUser);
        }

        private bool IgnoreRoutePermission(string ThisRoute)
        {
            try
            {
                return IgnoreRouteForPermission().Any(xxx => xxx == ThisRoute);
            }
            catch { return false; }
        }

        public void LogOut_User()
        {
            ClearAllCookies(this.HttpContext);
        }

        public void ClearAllCookies(HttpContextBase context)
        {
            var cookiesName = GetCoockieName;
            for (int i = 0; i < context.Request.Cookies.Count; i++)
            {
                HttpCookie currentUserCookie = context.Request.Cookies[i];
                if (currentUserCookie != null && currentUserCookie.Name.IndexOf(cookiesName) != -1)
                {
                    context.Response.Cookies.Remove(currentUserCookie.Name);
                    currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                    currentUserCookie.Value = null;
                    context.Response.SetCookie(currentUserCookie);
                }
            }
        }

        public List<string> IgnoreRouteForPermission()
        {
            // список игнорируемый маршрутов  (хранится в веб конфиге, через запятую в формате Controller_Action)
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings["IgnoreAction"].ToString().ToLower().Split(new char[] { ',' }).ToList();
            }
            catch { return null; }
        }

        public void CheckForExsistenceOfUserPermission()
        {
            bool NotFoundPermission = true;
            string[] IgnoreAction = System.Configuration.ConfigurationManager.AppSettings["IgnoreAction"].ToString().ToLower().Split(new char[] { ',' });
            string controllerName = this.GetType().Name.Replace("Controller", "").ToLower().ToString();
            string actionName = this.Request.RequestContext.RouteData.GetRequiredString("action").ToLower().ToString();
            string PermissionName = controllerName + "_" + actionName;

            NotFoundPermission = (!IgnoreRoutePermission(PermissionName));
            // проверяем список игнорируемых (список хранится в web.config AppSettings["IgnoreAction"].value)
          
            // если в списке игнорируемых не найден, то лезем в БД.
            if (NotFoundPermission)
            {

                var listPermission = _BD_.userpermission
                    .Where(xxx => xxx.Name == PermissionName)
                    .Select(xx => xx.Name.ToLower()).ToList();

                // проверяем наличии доступа в БД. // таблица содержит список контролеров + акшенов // на момент разработки не более 25. (запрос пыполнится моментально)

                if (listPermission == null || listPermission.Count() == 0)
                {
                    // если в БД не найден пермишн, нам надо его добавить

                    var permishion = new userpermission();
                    var url = this.Url.Action(actionName, controllerName);
                    permishion.Description = "Доступ к странице <a href='" + url + "'>" + PermissionName + "</a>";
                    permishion.Name = PermissionName;

                    _BD_.Entry(permishion).State = System.Data.Entity.EntityState.Added;
                    _BD_.SaveChanges();
                    //  и дать доступ всем пользователям (которые первыми зарегистрировались и подтвердили регистрацию от каждого Производителя.)
                    // у данных пользователей есть пермишн, Admin_Admin (он может быть не только у первых зарегистрированных а так же у пользователей, которым дали данный доступ)

                    List<produceruser> UsersProducerUser = _BD_.produceruser.Where(xxx => xxx.Enabled == 1).ToList();

                    List<produceruser> AdminPermission = new List<produceruser>();
                    foreach (var x in UsersProducerUser)
                    {
                        string AdminPermissonName = System.Configuration.ConfigurationManager.AppSettings["AdminActionName"].ToString();
                        long AdminPermissionId = _BD_.userpermission.Where(xxx => xxx.Name.ToLower().Contains(AdminPermissonName.ToLower())).Select(xxx => xxx.Id).First();
                        bool PermissionAdmin = _BD_.usertouserrole.Any(xxx => xxx.Id == AdminPermissionId && xxx.ProducerUserId == x.Id);

                        if (PermissionAdmin)
                        {
                            AdminPermission.Add(x);
                        }

                    }

                       // .GroupBy(xxx => xxx.ProducerId).Select(xxx => xxx.OrderByDescending(p => p.ProducerId).FirstOrDefault()).ToList();

                    long idPermission = _BD_.userpermission.Where(xxx => xxx.Name == PermissionName).First().Id;


                    foreach (var User in AdminPermission)
                    {
                        long IdUser = User.Id;
                        usertouserrole NewRole = new usertouserrole();
                        NewRole.UserPermissionId = idPermission;
                        NewRole.ProducerUserId = User.Id;
                        _BD_.usertouserrole.Add(NewRole);
                    }

                    _BD_.SaveChanges();
                }
            }
        }

        public produceruser GetCurrentUser(bool getFormSession = true)
        {
            if (AutorizedUser == null || (AutorizedUser.Name == String.Empty))
            {
                CurrentUser = null;
                return null;
            }
            if (getFormSession && (CurrentUser == null || CurrentUser.Name != CurrentUser.Email))
            {
                //  string Name = 
                CurrentUser = _BD_.produceruser.FirstOrDefault(e => e.Email == AutorizedUser.Name);
            }
            return CurrentUser;



        }       

        public string GetRandomPassword()
        {      
            return Guid.NewGuid().ToString().Replace("-", "").ToLower().Substring(8, MaxPasswordLeight);
        }

    }
}