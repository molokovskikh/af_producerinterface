using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.LoggerModels
{


    [MetadataType(typeof(logchangeviewMetaData))]
    public partial class logchangeview
    {
        public EntityCommand EntityCommandEnum
        {
            get {             
                return (EntityCommand)Action;
            }
            set { Action = (int)value; }
        }
          
    }
    public class logchangeviewMetaData
    {
        [Display(Name = "Статус")]
        public EntityCommand EntityCommandEnum { get; }
    }

}
