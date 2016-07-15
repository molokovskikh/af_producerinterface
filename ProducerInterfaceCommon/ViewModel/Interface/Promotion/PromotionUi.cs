using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using ProducerInterfaceCommon.Models;

namespace ProducerInterfaceCommon.ViewModel.Interface.Promotion
{
	public class PromotionUi
	{
		public string Title { get; set; } /* Новая промоакция или редактирование */
		public long Id { get; set; }
		public string Name { get; set; }
		public string Annotation { get; set; }
		public string Begin { get; set; }
		public string End { get; set; }
		public bool AllSuppliers { get; set; }
		public ActualPromotionStatus ActualStatus { get; set; }

		public System.Web.HttpPostedFileBase File { get; set; }

		public List<string> DrugList { get; set; }
		public List<string> RegionList { get; set; }
		public List<string> SuppierRegions { get; set; }

		public List<TextValue> DrugCatalogList { get; set; }
		public List<TextValue> RegionGlobalList { get; set; }
		public List<TextValue> SuppierRegionsList { get; set; }

		public long? PromotionFileId { get; set; }
		public string PromotionFileName { get; set; }
		public string PromotionFileUrl { get; set; }
	}

	public class TextValue
	{
		public string Value { get; set; }
		public string Text { get; set; }
	}
}