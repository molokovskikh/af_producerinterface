using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PermissionController : BaseController
    {
        // GET: Permission
        public ActionResult Index()
        {
            string AdministrationGroupname = GetWebConfigParameters("AdminGroupName");

            var ListAdministrators = cntx_.ControlPanelGroup.Where(xxx => xxx.Name == AdministrationGroupname).First().ControlPanelUser.Where(xxx=>xxx.Enabled==1).ToList();        

            return View(ListAdministrators);
        }

        //<a href = "~/Permission/Users" class="btn btn-primary">Список пользователей</a>
        //  <br />
        //  <a href = "~/Permission/Group" class="btn btn-primary">Список групп</a>

        public ActionResult Group()
        {
            var ListGroup = cntx_.ControlPanelGroup.ToList()
                .Select(xxx => new Models.ListGroupView {
                    Id = xxx.Id,
                    CountUser = xxx.ControlPanelUser.Where(eee => eee.Enabled == 1).Count(),
                    NameGroup = xxx.Name,
                    Users = xxx.ControlPanelUser.Where(zzz => zzz.Enabled == 1).Select(zzz => zzz.Name).ToArray(),
                    Permissions = xxx.ControlPanelPermission.Where(zzz => zzz.Enabled == true).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray()
                });

            return View(ListGroup);
        }

        public ActionResult Users()
        {
            var ListUser = cntx_.ControlPanelUser.ToList().
                Select(xxx => new Models.ListUserView {
                    Id = xxx.Id,
                    Name = xxx.Name,                  
                    Groups = xxx.ControlPanelGroup.Where(eee => eee.Enabled == true).Select(eee => eee.Name).ToArray(),
                    CountGroup = xxx.ControlPanelGroup.Where(eee => eee.Enabled == true).Count()
                });
            return View(ListUser);
        }


    }
}