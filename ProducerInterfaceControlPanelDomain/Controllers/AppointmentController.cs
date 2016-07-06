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
			var model = DB.AccountAppointment.OrderByDescending(x => x.GlobalEnabled).ToList();
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
			var appointment = DB.AccountAppointment.Find(Id);
			appointment.Name = Name;
			appointment.GlobalEnabled = GlobalEnabled;
			DB.SaveChanges();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Удаление должности
		/// </summary>
		/// <param name="id">идентификатор должности</param>
		/// <returns></returns>
		public ActionResult Delete(int id)
		{
			var users = DB.Account.Where(x => x.AppointmentId == id).ToList();
			foreach (var user in users)
				user.AppointmentId = null;
			DB.AccountAppointment.Remove(DB.AccountAppointment.Find(id));
			DB.SaveChanges();
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
			DB.AccountAppointment.Add(appointment);
			DB.SaveChanges();
			return RedirectToAction("Index");
		}
	}
}