using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.SPAM
{
    public class Enums
    {
        public enum TypeSpam
        {
            [Display(Name="Все события")]
            All = 0,

            [Display(Name = "Регистрация нового пользователя")]
            Registration =1,

            [Display(Name = "Пользователь подтвердил свой почтовый адрес")]
            RegistrationConfirm =11,

            [Display(Name = "Заявка на регистрацию, от анонимного пользователя")]
            RequestRegistration = 2,

            [Display(Name = "Успешная регистрация анонимного пользователя")]
            ReguestSuccessRegistration =3,

            [Display(Name = "Создание промоакции")]
            PromotionCreate = 4,

            [Display(Name = "Изменения промоакции")]
            PromotionChange = 5,

            [Display(Name = "Удаление промоакции")]
            PromotionDelete = 6,

            [Display(Name = "Подтверждение промоакции администратором")]
            PromotionSuccesss = 7,

            [Display(Name = "Создание новости")]
            NewsCreate = 8,

            [Display(Name = "Перемещение новости в архив")]
            NewsArchive = 9,

            [Display(Name = "Удаление новости")]
            NewsDelete = 10           
        }
    }

    public class Attributes
    {
        public string key { get; set; }
        public object Value { get; set; }
    }

}
