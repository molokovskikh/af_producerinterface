using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterface.Controllers
{
    public class UserPermissionController : MasterBaseController
    {
        // GET: UserPermission
        public ActionResult Index()
        {         
            var ListUser = cntx_.ProducerUser.Where(xxx => xxx.ProducerId == CurrentUser.ProducerId && xxx.TypeUser == SbyteTypeUser).ToList();
            return View(ListUser);
        }

        [HttpGet]
        public ActionResult Edit(long Id)
        {          
            var EditUser = cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First();

            // проверяем верные ли Id продюсера

            if (CurrentUser.ProducerId != EditUser.ProducerId)
            {
                ErrorMessage("У вас нет прав редактирования доступов для данного пользователя");
                return RedirectToAction("Index");
            }
            EditUser.UserPermission = cntx_.usertouserrole.Where(xxx => xxx.ProducerUserId == EditUser.Id).ToList().Select(yyy =>(long) yyy.UserPermissionId).ToList();
            ViewBag.ListPermission = cntx_.UserPermission.ToList().Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();

            return View(EditUser);
        }
        [HttpPost]
        public ActionResult Edit(ProducerUser ChangeUser)
        {
                     
            var EditUser = cntx_.ProducerUser.Where(xxx => xxx.Id == ChangeUser.Id).First();

            if (CurrentUser.ProducerId != EditUser.ProducerId)
            {
                ErrorMessage("У вас нет прав редактирования доступов для данного пользователя");
                return RedirectToAction("Index");
            }

            var PermissionOld = cntx_.usertouserrole.Where(xxx => xxx.ProducerUserId == EditUser.Id).ToList().Select(yyy => (long)yyy.UserPermissionId).ToList();

            // удаляем пермишены
            foreach (var PermissionItem in PermissionOld)
            {
                bool IfElsePermission = ChangeUser.UserPermission.Any(xxx => xxx == PermissionItem);

                if (!IfElsePermission)
                {
                    // если отсутствует пермишен
                    // удаляем в БД

                    var PermissionDelete = cntx_.usertouserrole.Where(xxx => xxx.UserPermissionId == PermissionItem && xxx.ProducerUserId == EditUser.Id).First();
                    cntx_.Entry(PermissionDelete).State = System.Data.Entity.EntityState.Deleted;
                }
            }
            cntx_.SaveChanges();

            // Добавляем пермишены
            foreach (var PermissionItem in ChangeUser.UserPermission)
            {
                bool IfElsePermission = PermissionOld.Any(xxx => xxx == PermissionItem);

                if (!IfElsePermission)
                {
                    // если в БД нет пермишена, добавляем
                    var NewPermission = new usertouserrole();
                    NewPermission.ProducerUserId = EditUser.Id;
                    NewPermission.UserPermissionId = PermissionItem;
                    cntx_.Entry(NewPermission).State = System.Data.Entity.EntityState.Added;                    
                }
            }
            cntx_.SaveChanges();

            SuccessMessage("Права пользователя " + EditUser.Name + " успешно изменены");
            return RedirectToAction("Index");
        }
    }
}