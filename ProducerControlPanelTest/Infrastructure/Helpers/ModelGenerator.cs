using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalitFramefork.Hibernate.Models;
using NHibernate;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerControlPanelTest.Infrastructure.Helpers
{

	//генерация моделей, на основе генератора для ProducerInterface
	public class ModelGenerator: ProducerInterfaceTest.Infrastructure.Helpers.ModelGenerator
	{
		private ISession DbSession { get; set; }

		public ModelGenerator(ISession dbSession) : base(dbSession)
		{
			DbSession = dbSession;
			// в строго заданном порядке
			Generate_Admin();
		}
		////////////////////////////////////|  ProducerControlPanel  |////////////////////////////////////////
		//Admin
		public Admin Clone_Admin(Admin admin = null)
		{
			if (admin == null)
			{
				DbSession.Flush();
				admin = DbSession.Query<Admin>().FirstOrDefault();
				if (admin == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста", typeof (Admin).Name));
				}
			}
			var newadmin = new Admin();
			newadmin.UserName = admin.UserName;
			newadmin.ManagerName = admin.ManagerName;
			newadmin.PhoneSupport = admin.PhoneSupport;
			newadmin.Email = admin.Email;
			DbSession.Save(newadmin);
			return newadmin;
		}

		public void Generate_Admin()
		{
			var admin = new Admin();
			admin.UserName = Environment.UserName;
			admin.ManagerName = "";
			admin.PhoneSupport = "3-333-333-3333";
			admin.Email = "mmm@mail.ru";
			DbSession.Save(admin);
			DbSession.Flush();

			admin = Clone_Admin(admin);
			admin.UserName = "Вотрой админ";
            DbSession.Save(admin);
			DbSession.Flush();

			admin = Clone_Admin(admin);
			admin.UserName = "Третий админ";
			DbSession.Save(admin);
			DbSession.Flush();
		}
		
	}
}