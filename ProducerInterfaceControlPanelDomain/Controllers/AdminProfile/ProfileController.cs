﻿using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class ProfileController : MasterBaseController
    {
        // GET: Profile
        public ActionResult Index()
        {      
            return View(CurrentUser);
        }

        [HttpPost]
        public ActionResult SaveProfile(Account UserModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index",UserModel);
            }

//            var CurrentUser = DB.ProducerUser.Where(xxx => xxx.Login == CurrentUser.Login).First();

            // если Id авторизованного пользователя и Id возвращаемое со страницы совпадают сохраняем в БД
            if (CurrentUser.Id == UserModel.Id)
            {
                //  CurrentUser.Login = UserModel.Login;

                var BdEmailAddress = DB.AccountEmail.Where(xxx => xxx.AccountId == UserModel.Id).FirstOrDefault();




                CurrentUser.Appointment = UserModel.Appointment;
                CurrentUser.Name = UserModel.Name;
                DB.Entry((Account)CurrentUser).State = System.Data.Entity.EntityState.Modified;
                DB.SaveChanges();

                SuccessMessage("Ваш профиль успешно сохранен");
                return RedirectToAction("Index", "Home");
            }

            ErrorMessage("Несанкционированный доступ");
            return RedirectToAction("Index", "Home");

        }


    }
}