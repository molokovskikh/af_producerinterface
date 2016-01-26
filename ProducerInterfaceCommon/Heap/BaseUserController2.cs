using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceCommon.Heap
{
    public class BaseController : Controller
    {
        protected producerinterface_Entities cntx_ = new producerinterface_Entities();


        public void CheckProducerInterfacePermission(string ControllerName, string ActionName, string Attributes = null)
        {
            // проверяем наличия пермишена

            bool PermissionExsist = cntx_.UserPermission.Any(xxx => xxx.Name == (ControllerName + "_" + ActionName));

            if (!PermissionExsist)
            {

                var NameGroupAdministration = GetWebConfigParameters("AdminGroupName");

                // проверяем наличие группы администраторы в БД           

                bool AdminGroupExsist = cntx_.userrole.Any(xxx => xxx.Name == NameGroupAdministration);

                var AdminGroup = new userrole();

                if (!AdminGroupExsist)
                {
                    // группы нет, создаём

                    AdminGroup.Description = "Администратор";
                    AdminGroup.Name = NameGroupAdministration;

                    cntx_.userrole.Add(AdminGroup);
                    cntx_.SaveChanges();
                }
                else
                {
                    // группа есть, присвиваем значение из БД
                    AdminGroup = cntx_.userrole.Where(xxx => xxx.Name == NameGroupAdministration).First();
                }

                var NewPermission = new UserPermission();
                NewPermission.Name = (ControllerName + "_" + ActionName).ToLower();
                NewPermission.Description = "новый доступ";
                cntx_.UserPermission.Add(NewPermission);
                cntx_.SaveChanges();

                var NewRoleToGroup = new usertouserrole();

                NewRoleToGroup.UserRoleId = AdminGroup.Id;
                NewRoleToGroup.UserPermissionId = NewPermission.Id;
                cntx_.usertouserrole.Add(NewRoleToGroup);
                cntx_.SaveChanges();
            }       
        }

        public void CheckControlPanelPermission(string ControllerName, string ActionName, string Attributes)
        {   
            // проверяем наличие пермишена в БД
            bool PermitionExsist = cntx_.ControlPanelPermission.Any(xxx => xxx.ControllerAction == (ControllerName + "_" + ActionName) && xxx.ActionAttributes.Contains(Attributes));

            if (!PermitionExsist)
            {
                // в базе не найдн пермишен для данного экшена

                var NewPermittion = new ControlPanelPermission();
                NewPermittion.ActionAttributes = Attributes;
                NewPermittion.ControllerAction = (ControllerName + "_" + ActionName);
                NewPermittion.Enabled = true;

                cntx_.ControlPanelPermission.Add(NewPermittion);
                cntx_.SaveChanges();

                string AdminGroupname = GetWebConfigParameters("AdminGroupName");
                bool AdminGroupExsist = cntx_.ControlPanelGroup.Any(xxx => xxx.Name == AdminGroupname);

                if (AdminGroupExsist)
                {
                    var AdminGroup = cntx_.ControlPanelGroup.Where(xxx => xxx.Name == AdminGroupname).First();
                    AdminGroup.ControlPanelPermission.Add(NewPermittion);
                    cntx_.SaveChanges();
                }
                else
                {
                    var NewAdminGroup = new ControlPanelGroup();
                    NewAdminGroup.Enabled = true;
                    NewAdminGroup.Name = AdminGroupname;
                    cntx_.ControlPanelGroup.Add(NewAdminGroup);
                    cntx_.SaveChanges();
                    NewAdminGroup.ControlPanelPermission.Add(NewPermittion);
                    cntx_.SaveChanges();
                }
            }

        }

        public void CheckReportsInterfacePermission(string ControllerName, string ActionName, string Attributes = null)
        {


        }

        public bool CheckProducerUserPermission()
        {
            return false;
        }

        public bool ChekControlPanelUserPermission()
        {
            return false;
        }

        public bool ChekReportsInterfaceUserPermission()
        {
            return false;
        }

        public void ClearAllCookies(HttpContextBase context)
        {
            var cookiesName = GetWebConfigParameters("UserCooliesAutentification");
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

        public string GetCookie(string cookieName)
        {
            try
            {
                var cookie = Request.Cookies.Get(cookieName);
                var base64EncodedBytes = Convert.FromBase64String(cookie.Value);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception e)
            {
                //IGNORE
                var x = e;
                return null;
            }
        }

        public void SetCookie(string name, string value)
        {
            if (value == null)
            {
                Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = DateTime.Now });
                return;
            }
            var plainTextBytes = Encoding.UTF8.GetBytes(value);
            var text = Convert.ToBase64String(plainTextBytes);
            Response.Cookies.Add(new HttpCookie(name, text) { Path = "/" });
        }

        public void DeleteCookie(string name)
        {
            Response.Cookies.Remove(name);
        }

        public void ClearAllCookies()
        {
            var cookiesName = System.Configuration.ConfigurationManager.AppSettings["CockieName"].ToString();
            for (int i = 0; i < Request.Cookies.Count; i++)
            {
                HttpCookie currentUserCookie = Request.Cookies[i];
                if (currentUserCookie != null && currentUserCookie.Name.IndexOf(cookiesName) != -1)
                {
                    Response.Cookies.Remove(currentUserCookie.Name);
                    currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                    currentUserCookie.Value = null;
                    Response.SetCookie(currentUserCookie);
                }
            }

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

        public class Md5HashHelper
        {
            /// <summary>
            /// Получение хэша строки
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public static string GetHash(string text)
            {
                using (MD5 md5Hash = MD5.Create()) return GetMd5Hash(md5Hash, text);
            }

            /// <summary>
            /// получение хэша строки
            /// </summary>
            /// <param name="md5Hash">Объект MD5</param>
            /// <param name="text">Хэшируемая строка</param>
            /// <returns>Хыш строки</returns>
            public static string GetMd5Hash(MD5 md5Hash, string text)
            {
                // Конвертация байтового массива в хэш
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                // создание строки
                StringBuilder sBuilder = new StringBuilder();
                // проходит по каждому байту хэша и форматирует его в 16 string
                for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
                return sBuilder.ToString();
            }

        }

        public string GetWebConfigParameters(string ParamKey)
        {
            return System.Configuration.ConfigurationManager.AppSettings[ParamKey].ToString();
        }

        public List<string> GetIgnoreRoute()
        {
            string IgnoreInWebConfig = GetWebConfigParameters("IgnoreRoute");
            List<string> Ret = IgnoreInWebConfig.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return new List<string>();
        }
    }
}
