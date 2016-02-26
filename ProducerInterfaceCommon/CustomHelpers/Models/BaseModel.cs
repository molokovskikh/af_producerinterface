using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.CustomHelpers.Models
{
    public abstract class BaseModel
    {      
            public abstract List<string> GetHeaders();

            public abstract string GetSpName();

            public abstract Dictionary<string, object> GetSpParams();

            public abstract Dictionary<string, object> ViewDataValues();
    }
}
