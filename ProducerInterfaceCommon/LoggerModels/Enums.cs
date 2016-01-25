using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.LoggerModels
{

    // EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
    // 4 8 16

    public enum EntityCommand
    {
        [Display(Name = "Добавлена запись")]
        ProductRatingReport = 2,

        [Display(Name = "Удалена запись")]
        PharmacyRatingReport = 4,

        [Display(Name = "Изменена запись")]
        SupplierRatingReport = 16
    }

}
