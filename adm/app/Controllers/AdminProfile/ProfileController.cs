using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class ProfileController : BaseController
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
                return View("Index",UserModel);

            // если Id авторизованного пользователя и Id возвращаемое со страницы совпадают сохраняем в БД
            if (CurrentUser.Id == UserModel.Id)
            {
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