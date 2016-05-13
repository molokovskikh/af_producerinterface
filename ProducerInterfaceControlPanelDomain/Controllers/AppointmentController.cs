using ProducerInterfaceCommon.ContextModels;
using System.Linq;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class AppointmentController : MasterBaseController
	{
		/// <summary>
		/// Список должностей
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = cntx_.AccountAppointment.OrderByDescending(x => x.GlobalEnabled).ToList();
			return View(model);
		}

		/// <summary>
		/// Изменение должности
		/// </summary>
		/// <param name="Id">идентификатор должности</param>
		/// <param name="Name">новое название должности</param>
		/// <returns></returns>
		public ActionResult Edit(int Id, string Name, bool GlobalEnabled)
		{
			var appointment = cntx_.AccountAppointment.Find(Id);
			appointment.Name = Name;
			appointment.GlobalEnabled = GlobalEnabled;
			cntx_.SaveChanges();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Удаление должности
		/// </summary>
		/// <param name="id">идентификатор должности</param>
		/// <returns></returns>
		public ActionResult Delete(int id)
		{
			var users = cntx_.Account.Where(x => x.AppointmentId == id).ToList();
			foreach (var user in users)
				user.AppointmentId = null;
			cntx_.AccountAppointment.Remove(cntx_.AccountAppointment.Find(id));
			cntx_.SaveChanges();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Добавление новой должности
		/// </summary>
		/// <param name="name">название должности</param>
		/// <returns></returns>
		public ActionResult Add(string name)
		{
			var appointment = new AccountAppointment() { Name = name, GlobalEnabled = true};
			cntx_.AccountAppointment.Add(appointment);
			cntx_.SaveChanges();
			return RedirectToAction("Index");
		}
	}
}