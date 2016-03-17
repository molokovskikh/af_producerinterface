using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ViewModel.Interface.Promotion
{
    public class PromotionEdit
    {

        public string Title { get; set; } /* Новая промоакция или редактирование */
        public long Id { get; set; }
        public string Name { get; set; }
        public string Annotation { get; set; }
        public string Begin { get; set; } /* DateTime Begin */
        public string End { get; set; } /* DateTime End*/
        public System.Web.HttpPostedFileBase File { get; set; }

        public List<long> DrugList { get; set; }
        public List<long> RegionList { get; set; }
        public List<decimal> SuppierRegions { get; set; }

        public List<TextValue> DrugCatalogList { get; set; }
        public List<TextValue> RegionGlobalList { get; set; }
        public List<TextValue> SuppierRegionsList { get; set; }

        public long PromotionFileId { get; set; }
        public string PromotionFileName { get; set; }
        public string PromotionFileUrl { get; set; }     
    }

    public class TextValue
    {
        public long Value { get; set; }
        public string Text { get; set; }
    }
}
