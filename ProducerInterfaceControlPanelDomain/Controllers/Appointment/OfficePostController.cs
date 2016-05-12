using ProducerInterfaceCommon.ContextModels;
using System.Linq;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.ProducersInformation
{
	public class OfficePostController : MasterBaseController
	{
		/// <summary>
		/// Список должностей
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = cntx_.AccountAppointment.ToList();
			return View(model);
		}

		/// <summary>
		/// Изменение статуса должности
		/// </summary>
		/// <param name="IdAppointment">идентификатор должности</param>
		/// <param name="Active">доступность в глобальном списке должностей</param>
		/// <returns></returns>
		public ActionResult GlobalAppointmentActivate(int IdAppointment, bool Active)
		{
			var ChangeAppointment = cntx_.AccountAppointment.Find(IdAppointment);
			ChangeAppointment.GlobalEnabled = Active;
			cntx_.Entry(ChangeAppointment).State = System.Data.Entity.EntityState.Modified;
			cntx_.SaveChanges();

			var model = cntx_.AccountAppointment.ToList();
			return View("Index", model);
		}

		/// <summary>
		/// Изменение названия должности
		/// </summary>
		/// <param name="idAppointment">идентификатор должности</param>
		/// <param name="nameAppointmentName">новое название должности</param>
		/// <returns></returns>
		public ActionResult GlobalAppointmentChange(int idAppointment, string nameAppointmentName)
		{
			var AppointmentChange = cntx_.AccountAppointment.Find(idAppointment);
			AppointmentChange.Name = nameAppointmentName;
			cntx_.SaveChanges();
			var model = cntx_.AccountAppointment.ToList();
			return View("Index", model);
		}

		/// <summary>
		/// Удаление должности
		/// </summary>
		/// <param name="idDelAppointment">идентификатор должности</param>
		/// <returns></returns>
		public ActionResult GlobalAppointmentDelete(int idDelAppointment)
		{

			var AccountList = cntx_.Account.Where(x => x.AppointmentId == idDelAppointment).ToList();

			foreach (var AccountItem in AccountList)
			{
				AccountItem.AppointmentId = null;
				cntx_.Entry(AccountItem).State = System.Data.Entity.EntityState.Modified;
			}
			cntx_.SaveChanges();

			cntx_.AccountAppointment.Remove(cntx_.AccountAppointment.Find(idDelAppointment));
			cntx_.SaveChanges(CurrentUser, "Удаление должности из глобального списка");

			var model = cntx_.AccountAppointment.ToList();
			return View("Index", model);
		}

		/// <summary>
		/// Добавление новой должности в глобальный список должностей
		/// </summary>
		/// <param name="NewNameAppointment">название должности</param>
		/// <returns></returns>
		public ActionResult GlobalAppointmentAdd(string NewNameAppointment)
		{
			var NewAppointment = new AccountAppointment();
			NewAppointment.Name = NewNameAppointment;
			NewAppointment.GlobalEnabled = true;

			cntx_.AccountAppointment.Add(NewAppointment);
			cntx_.SaveChanges();
			var model = cntx_.AccountAppointment.ToList();
			return View("Index", model);
		}
	}
}