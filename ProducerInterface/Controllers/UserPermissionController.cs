using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class UserPermissionController : pruducercontroller.BaseController
    {
        // GET: UserPermission
        public ActionResult Index()
        {
            var User = GetCurrentUser();
            var ListUser = _BD_.produceruser.Where(xxx => xxx.ProducerId == User.ProducerId).ToList();

            return View(ListUser);
        }

        [HttpGet]
        public ActionResult Edit(long Id)
        {
            var User = GetCurrentUser();
            var EditUser = _BD_.produceruser.Where(xxx => xxx.Id == Id).First();

            // проверяем верные ли Id продюсера

            if (User.ProducerId != EditUser.ProducerId)
            {
                ErrorMessage("У вас нет прав редактирования доступов для данного пользователя");
                return RedirectToAction("Index");
            }
            EditUser.UserPermission = _BD_.usertouserrole.Where(xxx => xxx.ProducerUserId == EditUser.Id).ToList().Select(yyy =>(long) yyy.UserPermissionId).ToList();
            ViewBag.ListPermission = _BD_.userpermission.ToList().Select(xxx => new Models.OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();

            return View(EditUser);
        }
        [HttpPost]
        public ActionResult Edit(Models.produceruser ChangeUser)
        {

            var User = GetCurrentUser();
            var EditUser = _BD_.produceruser.Where(xxx => xxx.Id == ChangeUser.Id).First();

            if (User.ProducerId != EditUser.ProducerId)
            {
                ErrorMessage("У вас нет прав редактирования доступов для данного пользователя");
                return RedirectToAction("Index");
            }

            var PermissionOld = _BD_.usertouserrole.Where(xxx => xxx.ProducerUserId == EditUser.Id).ToList().Select(yyy => (long)yyy.UserPermissionId).ToList();

            // удаляем пермишены
            foreach (var PermissionItem in PermissionOld)
            {
                bool IfElsePermission = ChangeUser.UserPermission.Any(xxx => xxx == PermissionItem);

                if (!IfElsePermission)
                {
                    // если отсутствует пермишен
                    // удаляем в БД

                    var PermissionDelete = _BD_.usertouserrole.Where(xxx => xxx.UserPermissionId == PermissionItem && xxx.ProducerUserId == EditUser.Id).First();
                    _BD_.Entry(PermissionDelete).State = System.Data.Entity.EntityState.Deleted;
                }
            }
            _BD_.SaveChanges();

            // Добавляем пермишены
            foreach (var PermissionItem in ChangeUser.UserPermission)
            {
                bool IfElsePermission = PermissionOld.Any(xxx => xxx == PermissionItem);

                if (!IfElsePermission)
                {
                    // если в БД нет пермишена, добавляем
                    var NewPermission = new Models.usertouserrole();
                    NewPermission.ProducerUserId = EditUser.Id;
                    NewPermission.UserPermissionId = PermissionItem;
                    _BD_.Entry(NewPermission).State = System.Data.Entity.EntityState.Added;                    
                }
            }
            _BD_.SaveChanges();

            SuccessMessage("Права пользователя " + EditUser.Name + " успешно изменены");
            return RedirectToAction("Index");
        }
    }
}