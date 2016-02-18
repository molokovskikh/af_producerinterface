using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProducerInterfaceCommon.Heap;
using Quartz;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
    [Serializable]
    public class ProductPriceDynamicsReport : IntervalReport
    {
        [Display(Name = "Регион")]
        [Required(ErrorMessage = "Не указаны регионы")]
        [UIHint("DecimalList")]
        public List<decimal> RegionCodeEqual { get; set; }
        
        [Display(Name = "Товар")]
        [Required(ErrorMessage = "Не выбраны товары")]
        [UIHint("LongList")]
        public List<long> CatalogIdEqual { get; set; }

        public override string Name
        {
            get { return "Динамика цен по товару за период"; }
        }

        public override List<string> GetHeaders(HeaderHelper h)
        {
            var result = new List<string>();
            result.Add(h.GetDateHeader(DateFrom, DateTo));
            result.Add(h.GetRegionHeader(RegionCodeEqual));
            result.Add(h.GetProductHeader(CatalogIdEqual));         
            return result;
        }

        public override string GetSpName()
        {
            return "ProductPriceDynamicsReport";
        }

        public override Dictionary<string, object> GetSpParams()
        {
            var spparams = new Dictionary<string, object>();
            spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
            spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));         
        
            spparams.Add("@DateFrom", DateFrom);
            spparams.Add("@DateTo", DateTo);
            return spparams;
        }

        public override Report Process(JobKey key, Report jparam, TriggerParam tparam)
        {
            var processor = new Processor<ProductPriceDynamicsReportRow>();
            processor.Process(key, jparam, tparam);
            return jparam;
        }

        public override Dictionary<string, object> ViewDataValues(NamesHelper h)
        {
            var viewDataValues = new Dictionary<string, object>();

            viewDataValues.Add("RegionCodeEqual", h.GetRegionList());
            viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());          

            return viewDataValues;
        }
    }
}
