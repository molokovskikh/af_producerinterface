using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class ProfileController : BaseController
    {
        // GET: Profile
        public ActionResult Index()
        {
            var UserModel = cntx_.ControlPanelUser.Where(xxx => xxx.Name == currentUser).First();
            return View(UserModel);
        }

        [HttpPost]
        public ActionResult SaveProfile(Models.ControlPanelUser UserModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index",UserModel);
            }

            var CurrentUser = cntx_.ControlPanelUser.Where(xxx => xxx.Name == currentUser).First();

            // если Id авторизованного пользователя и Id возвращаемое со страницы совпадают сохраняем в БД
            if (CurrentUser.Id == UserModel.Id)
            {
                CurrentUser.Email = UserModel.Email;
                CurrentUser.Appointment = UserModel.Appointment;
                
                cntx_.Entry(CurrentUser).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges();

                SuccessMessage("Ваш профиль успешно сохранен");
                return RedirectToAction("Index", "Home");
            }

            ErrorMessage("Несанкционированный доступ");
            return RedirectToAction("Index", "Home");

        }
    }
}