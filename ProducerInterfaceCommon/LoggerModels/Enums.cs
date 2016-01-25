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
        [Display(Name ="Добавлена запись")]
        Added = 4,

        [Display(Name = "Удалена запись")]
        Deleted = 8,

        [Display(Name = "Изменена запись")]
        Modified = 16
    }

    public static class DescriptionHelper
    {
        // Получение значения атребута "Description"
        public static string GetDescription(this object self, string fieldName = "")
        {
            //обработка полей перечислений
            if (self as Enum != null)
            {
                var fieldInfoEnum = self.GetType().GetField(self.ToString());
                if (fieldInfoEnum != null)
                {
                    var attributesEnum =
                     (DisplayAttribute[])fieldInfoEnum.GetCustomAttributes(typeof(DisplayAttribute), false);
                    return (attributesEnum.Length > 0) ? attributesEnum[0].Name : self.ToString();
                }
            }
            // обработка самой модели 
            if (self != null && fieldName == string.Empty)
            {
                var modelAttributes =
                 (DisplayAttribute[])self.GetType().GetCustomAttributes(typeof(DisplayAttribute), false);
                return (modelAttributes.Length > 0) ? modelAttributes[0].Name : self.GetType().Name;
            }
            // обработка полей модели, полей с атрибутом описания
            var property = self.GetType().GetProperty(fieldName);
            var attributes = (DisplayAttribute[])property.GetCustomAttributes(typeof(DisplayAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Name : fieldName;
        }
    }


}
