using System;
using System.Linq;
using System.Web.SessionState;
using NHibernate;
using NHibernate.Linq;
using Test.Support;

namespace ProducerInterface.Test.Tasks
{
	public class Fixture
	{
		public void Orders()
		{
			var factory = global::Test.Support.Setup.Initialize("db");
			using (var session = factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				var order = TestOrder.CreateSimple(session);
				var catalog = session.Query<TestAssortment>().First(x => !x.Catalog.Hidden && x.Catalog.Pharmacie).Catalog;
				var product = session.Query<TestProduct>().First(x => x.CatalogProduct == catalog);
				order.AddItem(product, 10, 100);
				order.WriteTime = DateTime.Now.AddDays(-7);
				session.Save(order);

				session.CreateSQLQuery(@"
INSERT INTO OrdersOld.OrdersHead (RowID, WriteTime, ClientCode, AddressId, UserId, PriceCode, RegionCode, PriceDate, SubmitDate)
select RowID, WriteTime, ClientCode, AddressId, UserId, PriceCode, RegionCode, PriceDate, SubmitDate
from Orders.OrdersHead
where RowId = :id;

INSERT INTO OrdersOld.OrdersList (OrderID, ProductId, CodeFirmCr, SynonymCode, SynonymFirmCrCode, Code, CodeCr, Quantity, Junk, RequestRatio, OrderCost, MinOrderCount, Cost)
SELECT ol.OrderID, ol.ProductId, ol.CodeFirmCr, ol.SynonymCode, ol.SynonymFirmCrCode, ol.Code, ol.CodeCr, ol.Quantity, ol.Junk, ol.RequestRatio, ol.OrderCost, ol.MinOrderCount, ol.Cost
FROM Orders.OrdersList Ol
WHERE Ol.OrderID = :id;")
					.SetParameter("id", order.Id)
					.SetFlushMode(FlushMode.Always)
					.ExecuteUpdate();

				trx.Commit();
			}
		}
	}
}