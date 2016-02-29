using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public partial class ProducerInformationController : MasterBaseController
    {
        public ActionResult GlobalListAppointment()
        {
            var AppointmentList = cntx_.AccountAppointment.Where(x => x.GlobalEnabled == 1).ToList();
            return View(AppointmentList);
        }

        public ActionResult GlobalAppointmentChange(int idAppointment, string nameAppointment)
        {

            return View();
        }


        // на вход приходит id удаляемой должности и id должности которую установить для пользователей которые данную должность выбрали
        public ActionResult GlobalAppointmentDelete(int idDelAppointment, int idSetAppointmentToAccounts)
        {

            return View();
        }

        public ActionResult GlobalAppointmentAdd(string NewNameAppointment)
        {
            var NewAppointment = new AccountAppointment();
            NewAppointment.Name = NewNameAppointment;
            NewAppointment.GlobalEnabled = 1;

            cntx_.AccountAppointment.Add(NewAppointment);
            cntx_.SaveChanges();

            var AppointmentListModel = cntx_.AccountAppointment.Where(x=>x.GlobalEnabled == 1).ToList();
            return PartialView("ListGlobalAppointment",AppointmentListModel);
        }

    }
}
