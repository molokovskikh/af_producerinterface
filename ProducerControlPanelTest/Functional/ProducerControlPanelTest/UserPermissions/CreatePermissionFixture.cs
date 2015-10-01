using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalitFramefork.Hibernate.Models;
using NHibernate.Driver;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerInterface.Models;

namespace ProducerControlPanelTest.Functional.ProducerControlPanelTest.UserPermissions
{
	class CreatePermissionFixture : UserPermissionsFixture
	{
		[Test, Description("Добавление права")]
		public void CreatePermission()
		{
			Open("UserPermissions/ListPermissions");

			Browser.FindElementByCssSelector("i.entypo-attach").Click();
			var permissionName = Browser.FindElementByCssSelector("input[id=currentPermission_Name]");
			var permissionDescription = Browser.FindElementByCssSelector("textarea[id=currentPermission_Description]");
			permissionName.SendKeys("Право на создание объектов");
			permissionDescription.SendKeys("Test-право, не для работы");
			Browser.FindElementByCssSelector(".btn.btn-success").Click();
			//AssertText("");
			var createPermission = DbSession.Query<UserPermission>().FirstOrDefault(p => p.Name == "Право на создание объектов");
			Assert.That(createPermission, Is.Not.Null, "Право должен сохраниться в базе данных");
		}
	}
}
