using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EntityContext.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class ProducerUserPermissionController : BaseController
    {
        // GET: ProducerUserPermission
        [HttpGet]
        public ActionResult Index(long Id=0)
        {
            var UserList = new List<ProducerUser>();
            if (Id == 0)
            {
                UserList = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1).ToList();
            }
            else
            {
                UserList = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.ProducerId == Id).ToList();
            }
            return View(UserList);
        }

        [HttpGet]
        public ActionResult EditUserPermission(long Id)
        {
            var UserModel = cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First();
            ViewBag.PermissionList = cntx_.UserPermission.ToList().Select(xxx => new OptionElement { Text=xxx.Name, Value= xxx.Id.ToString() }).ToList();
            UserModel.ListSelectedPermission = (List<long>) cntx_.usertouserrole.Where(xxx => xxx.ProducerUserId == UserModel.Id).Select(xxx =>(long) xxx.UserPermissionId).ToList();


            return View("Edit", UserModel);
        }

        [HttpPost]
        public ActionResult EditUserPermission(ProducerUser ChangeUser)
        {
            // список пермишенов в Базе
            var UselPermissionOldList = cntx_.usertouserrole.Where(xxx => xxx.ProducerUserId == ChangeUser.Id).Select(xxx =>(long) xxx.UserPermissionId).ToList();


            //удаление пермишена
            foreach (var UselPermissionOldListItem in UselPermissionOldList)
            {
                bool SelectNoSelectPermission = ChangeUser.ListSelectedPermission.Any(xxx => xxx == UselPermissionOldListItem); // проверяем, содержит ли входящий список данный доступ

                if (!SelectNoSelectPermission)
                {
                    // если не содержит, адуляем из Базы

                    var DeletePermission = cntx_.usertouserrole.Where(xxx => xxx.ProducerUserId == ChangeUser.Id && xxx.UserPermissionId == UselPermissionOldListItem).First();

                    cntx_.usertouserrole.Remove(DeletePermission);
                   
                }
                else
                {
                    // пермишен остался на месте
                }
            }
            cntx_.SaveChanges();

            //добавление пермишенов
            foreach (var NewItemPermission in ChangeUser.ListSelectedPermission)
            {

                var SelectNotSelectPermission = UselPermissionOldList.Any(xxx=>xxx == NewItemPermission);

                if(!SelectNotSelectPermission)
                {
                    // В бд нет данного пермишена
                    var PermissionAdd = new usertouserrole();
                    PermissionAdd.ProducerUserId = ChangeUser.Id;
                    PermissionAdd.UserPermissionId = NewItemPermission;
                    cntx_.usertouserrole.Add(PermissionAdd);
                }
            }
            cntx_.SaveChanges();

            SuccessMessage("Успешное обновление прав " + ChangeUser.Name);
            return RedirectToAction("Index");
        }
        

        [HttpGet]
        public ActionResult ListPermission()
        {

            var ListPermission = cntx_.UserPermission.ToList();

            return View(ListPermission);
        }

        [HttpGet]
        public ActionResult EditPermission(long Id)
        {
            var PermissionItem = cntx_.UserPermission.Where(xxx => xxx.Id == Id).First();

            return View(PermissionItem);
        }
        [HttpPost]
        public ActionResult EditPermission(UserPermission ChangePermission)
        {
            var PermissionChange = cntx_.UserPermission.Where(xxx => xxx.Id == ChangePermission.Id).First();

            // сохраняем только изменённое описание для данного пермишена

            PermissionChange.Description = ChangePermission.Description;
            cntx_.Entry(PermissionChange).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Описание " + ChangePermission.Name + " выполнено успешно");
            return RedirectToAction("ListPermission");
        }

    }
}