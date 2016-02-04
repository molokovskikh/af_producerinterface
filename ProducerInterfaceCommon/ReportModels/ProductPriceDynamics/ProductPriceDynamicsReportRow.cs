using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.Models
{
    public class ProductPriceDynamicsReportRow : RatingReportRow
    {
        [Display(Name = "Регион")]
        public string RegionName { get; set; }

        [Display(Name = "Наименование и форма выпуска")]
        public string CatalogName { get; set; }
    }
}
