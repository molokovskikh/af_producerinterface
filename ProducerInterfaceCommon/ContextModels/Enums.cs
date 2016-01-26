using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ContextModels
{
    public enum DisplayStatus
    {

        [Display(Name = "Не запускался")]
        New = 0,

        [Display(Name = "Отчёт готовится")]
        Processed = 1,

        [Display(Name = "Отчёт готов")]
        Ready = 2,

        [Display(Name = "Нет данных для построения отчёта")]
        Empty = 3,

        [Display(Name = "В процессе подготовки произошла ошибка")]
        Error = 4
    }

    public enum Reports
    {
        [Display(Name = "Рейтинг товаров")]
        ProductRatingReport = 1,

        [Display(Name = "Рейтинг аптек")]
        PharmacyRatingReport = 2,

        [Display(Name = "Рейтинг поставщиков")]
        SupplierRatingReport = 3
    }

    public enum TypeUsers
    {
        [Display(Name = "Пользователь из Интерфейса производителя")]
        ProducerUser =0,
        [Display(Name = "Пользователь из Панели управления")]
        ControlPanelUser =1,
        [Display(Name = "Пользователь из Из интерфейса отчетов")]
        ReportUser =2,
        [Display(Name = "Для тестов")]
        UserRazrab =10
    }

}
