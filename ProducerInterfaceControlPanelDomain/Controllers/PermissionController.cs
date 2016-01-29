using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PermissionController : BasePermissionController
    {
        // GET: Permission

        public PermissionController()
        {
            FilterType = TypeUsers.ControlPanelUser;
        }  
    }
}