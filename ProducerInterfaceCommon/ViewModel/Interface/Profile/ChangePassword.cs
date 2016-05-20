using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ViewModel.Interface.Profile
{
    public class ChangePassword
    {
        [Display(Name="Пароль")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Пароль может содержать только латинские буквы и цифры")]
        [MinLength(6, ErrorMessage ="Минимальная длина пароля 6 символов")]
        public string Pass { get; set; }
       
        [Display(Name = "Подтверждение пароля")]        
        [Compare("Pass", ErrorMessage = "Пароль и подтверждение пароля не совпадают")]
        public string PassConfirm { get; set; }
    }
}
