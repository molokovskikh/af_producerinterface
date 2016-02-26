using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Registration
{
    /* модель валидации при входе */ 
    public class AuthViewModel
    {
        [UIHint("EditorMail")]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string login { get; set; }

        [UIHint("Editor_Password")]
        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Введите пароль")]
        public string password { get; set; }
    }
}
