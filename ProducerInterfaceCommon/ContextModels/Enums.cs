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

        [Display(Name = "Отчет готовится")]
        Processed = 1,

        [Display(Name = "Отчет готов")]
        Ready = 2,

        [Display(Name = "Нет данных для построения отчета")]
        Empty = 3,

        [Display(Name = "В процессе подготовки произошла ошибка")]
        Error = 4
    }
     

    public enum NewsChanges
    {
        [Display(Name ="Добавление")]
        NewsAdd = 0,
        [Display(Name = "Редактирование")]
        NewsChange =1,
        [Display(Name = "Отправлено в архив")]
        NewsArchive = 2       
    }

    public enum Reports
    {
        [Display(Name = "Рейтинг товаров")]
        ProductRatingReport = 1,

        [Display(Name = "Рейтинг аптек")]
        PharmacyRatingReport = 2,

        [Display(Name = "Рейтинг поставщиков")]
        SupplierRatingReport = 3,

        [Display(Name = "Рейтинг товаров в конкурентной группе")]
        ProductConcurentRatingReport = 4,

        [Display(Name= "Динамика цен по товару за период")]
        ProductPriceDynamicsReport = 5
    }

    public enum TypeUsers
    {
        [Display(Name = "Пользователь из Интерфейса производителя")]
        ProducerUser =0,
        [Display(Name = "Пользователь из Панели управления")]
        ControlPanelUser =1,
        [Display(Name = "Пользователь из интерфейса производителя, без Id производителя")]
        UserNotProducer =2    
    }

    public enum EntityCommand
    {
        [Display(Name = "Добавлено")]
        Added = 4,

        [Display(Name = "Удалено")]
        Deleted = 8,

        [Display(Name = "Изменено")]
        Modified = 16
    }

}
