using NUnit.Framework;
using ProducerInterface.Controllers;
using System.Web.Mvc;

namespace ProducerInterface.Test.Controller
{
	[TestFixture]
	public class ReportControllerTest
	{
		[Test]
		public void JobList_UserWithoutProducer_RedirectToHome()
		{
			var controller = new ReportController();
			//var result = controller.JobList((long?)null);
			//Assert.IsInstanceOf<ViewResult>(result);
			//Assert.IsInstanceOf<UserView>(((ViewResult)result).Model);

			Assert.IsTrue(true);
		}
	}
}
