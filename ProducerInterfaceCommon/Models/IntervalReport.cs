using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Tools;
using Dapper;
using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public abstract class IntervalReport : Report, IInterval
	{
		[ScaffoldColumn(false)]
		public DateTime DateFrom { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateTo { get; set; }

		protected string GetRegions(MySqlConnection connection, List<decimal> regions)
		{
			var regionIds = String.Join(", ", regions);
			var children = connection.Query<ulong>($"select RegionCode from Farm.Regions where Parent in ({regionIds})").ToList();
			return regions.Select(x => Convert.ToUInt64(x)).Concat(children).Implode();
		}

		protected string GetCatalogHeader(HeaderHelper h, bool allCatalog, List<long> catalogIdEqual)
		{
			// если выбрано По всем нашим товарам
			if (allCatalog) {
				if (ProducerId == null)
					return "В отчет включены все товары";
				else
					return "В отчет включены все товары производителя";
			} else {
				return h.GetProductHeader(catalogIdEqual);
			}
		}
	}
}