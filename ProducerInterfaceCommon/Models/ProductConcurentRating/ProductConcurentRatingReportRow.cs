using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.Models
{
    class ProductConcurentRatingReportRow : RatingReportRow
    {
        [Display(Name = "Наименование")]
        public string CatalogName { get; set; }
    }
}
