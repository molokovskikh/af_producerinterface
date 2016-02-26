using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Registration
{
    public class RegProducerViewModel
    {
        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Выберите вашу компанию: ")]
        public long ProducerId { get; set; }
    }
}
