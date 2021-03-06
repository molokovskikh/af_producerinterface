﻿using System.Linq;
using NUnit.Framework;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class ModelFixture
	{
		[Test]
		public void Load_regions()
		{
			var cntx = new producerinterface_Entities();
			var regions =  cntx.Regions();
			Assert.That(regions.Count, Is.GreaterThan(0));
			Assert.That(regions[0].Id, Is.GreaterThan(0));
		}

		[Test]
		public void Load_promotions()
		{
			var db = new Context();
			Assert.That(db.Set<Promotion>().ToList().Count, Is.GreaterThan(0));
		}
	}
}