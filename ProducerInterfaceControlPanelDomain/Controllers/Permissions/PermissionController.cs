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
        ProducerInterfaceCommon.ContextModels.TypeUsers TypeLoginUserThis { get { return (ProducerInterfaceCommon.ContextModels.TypeUsers)SbyteTypeUser_; } set { SbyteTypeUser_ = (SByte)value; } }
        protected SByte SbyteTypeUser_ { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            FilterType = TypeUsers.ControlPanelUser;
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Group()
        {
            var ListGroup = cntx_.AccountGroup.Where(xxx => xxx.TypeGroup == FilterSbyte).ToList()
                .Select(xxx => new ListGroupView
                {
                    Id = xxx.Id,
                    CountUser = xxx.Account.Where(eee => eee.Enabled == 1 && eee.TypeUser == FilterSbyte).Count(),
                    NameGroup = xxx.Name,
                    Description = xxx.Description,
                    Users = xxx.Account.Where(zzz => zzz.TypeUser == FilterSbyte && zzz.Enabled == 1 && zzz.TypeUser == FilterSbyte).Select(zzz => zzz.Name).ToArray(),
                    Permissions = xxx.AccountPermission.Where(zzz => zzz.Enabled == true && zzz.TypePermission == FilterSbyte).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray()
                });

            return View(ListGroup);
        }


    }
}