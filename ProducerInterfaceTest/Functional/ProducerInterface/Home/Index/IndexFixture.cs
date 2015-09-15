using AnalitFramefork.Tests;
using NUnit.Framework;
using ProducerInterfaceTest.Infrastructure;

namespace ProducerInterfaceTest.Functional.Home.Index
{
	[TestFixture]
	class IndexFixture : BaseFixture
	{
		[Test]
		public void VisualTest()
		{ 
			LoginForUser();
			AssertText("Кабинет произодителя");
		}
	}
}
