using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.Controllers
{
    public class AccountGroupPermissionChanges
    {

        private ProducerInterfaceCommon.ContextModels.producerinterface_Entities _cntx_;
     
        public AccountGroupPermissionChanges(producerinterface_Entities cntx_, int GroupId, List<long> NewUserList, List<int> NewPermissionList)
        {
            this._cntx_ = cntx_;         
        }
        
    


    }

   
}
