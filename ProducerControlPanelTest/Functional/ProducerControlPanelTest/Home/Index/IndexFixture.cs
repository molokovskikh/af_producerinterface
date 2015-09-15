using System;
using System.Linq;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Tests;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerControlPanelTest.Infrastructure;
using ProducerInterfaceTest.Infrastructure;

namespace ProducerControlPanelTest.Functional.Home.Index
{
	[TestFixture]
	class IndexFixture : BaseFixture
	{
		[Test]
		public void VisualTest()
		{ 
			LoginForAdmin();
			AssertText("Выход");
		}
	}
}
