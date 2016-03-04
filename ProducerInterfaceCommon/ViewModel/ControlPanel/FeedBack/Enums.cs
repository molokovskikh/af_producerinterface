using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
    public class Enums
    {
        public enum FeedBackStatus
        {
            [Display(Name="Не обработана")]
            Begin = 0,
            [Display(Name = "В работе")]
            Work = 1,
            [Display(Name = "Завершена")]
            End = 2,
            [Display(Name = "Помечена на удаление, не актуальна")]
            Fail = 3
        }
    }
}
