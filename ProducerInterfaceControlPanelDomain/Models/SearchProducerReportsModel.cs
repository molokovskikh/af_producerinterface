using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProducerInterfaceControlPanelDomain.Models
{
    public class SearchProducerReportsModel
    {
        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Название компании производителя: ")]
        public long Producers { get; set; }
    }
}