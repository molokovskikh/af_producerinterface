using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PermissionUserController : MasterBaseController
    {    
        ProducerInterfaceCommon.ContextModels.TypeUsers TypeLoginUserThis { get { return (ProducerInterfaceCommon.ContextModels.TypeUsers)SbyteTypeUser_; } set { SbyteTypeUser_ = (SByte)value; } }
        protected SByte SbyteTypeUser_ { get; set; }

        public PermissionUserController()
        {
            TypeLoginUserThis = TypeUsers.ProducerUser;
        }

        public ActionResult Index()
        {
            string AdministrationGroupname = GetWebConfigParameters("AdminGroupName");

            var ListAdministrators = cntx_.ControlPanelGroup.Where(xxx => xxx.Name == AdministrationGroupname && xxx.TypeGroup == SbyteTypeUser_).First().ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser_).ToList();
            return View(ListAdministrators);
        }

        public ActionResult Group()
        {
        
            var ListGroup = cntx_.ControlPanelGroup.Where(xxx => xxx.TypeGroup == SbyteTypeUser_).ToList()
                .Select(xxx => new ListGroupView
                {
                    Id = xxx.Id,
                    CountUser = xxx.ProducerUser.Where(eee => eee.Enabled == 1 && eee.TypeUser == SbyteTypeUser_).Count(),
                    NameGroup = xxx.Name,
                    Description = xxx.Description,
                    Users = xxx.ProducerUser.Where(zzz => zzz.TypeUser == SbyteTypeUser_ && zzz.Enabled == 1).Select(zzz => zzz.Name).ToArray(),
                    Permissions = xxx.ControlPanelPermission.Where(zzz => zzz.Enabled == true && zzz.TypePermission == SbyteTypeUser_).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray(),
                    ListUsersInGroup = xxx.ProducerUser.Where(zzz=>zzz.TypeUser == SbyteTypeUser_ && zzz.Enabled==1).Select(v=> new UsersViewInChange { Name = v.Name, eMail = v.Email, ProducerName = v.ProducerId.ToString() }).ToList()
                });
            ProducerInterfaceCommon.Heap.NamesHelper h;
            h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.GetProducerList();
            
            return View(ListGroup);
        }

        public ActionResult Users()
        {
       
            var ModelListUserView = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.Login != null).ToList()
                .Select(vvv => new ListUserView
                {
                    Id = vvv.Id,
                    Name = vvv.Name,
                    Groups = cntx_.ControlPanelGroup.ToList().Where(zzz => zzz.ProducerUser.Any(zzzz => zzzz.Id == vvv.Id && zzzz.TypeUser == SbyteTypeUser_)).ToList().Select(z => z.Name).ToArray(),
                    ListPermission = cntx_.ControlPanelPermission.ToList().Where(zzz =>zzz.TypePermission == SbyteTypeUser_ && zzz.ControlPanelGroup.Any(ccc => ccc.ProducerUser.Any(v => v.Id == vvv.Id && v.TypeUser == SbyteTypeUser_))).ToList().Select(vv => vv.ControllerAction + "   " + vv.ActionAttributes).ToArray()
                }).ToList();

            foreach (var ItemModel in ModelListUserView)
            {
                ItemModel.CountGroup = ItemModel.Groups.Length;
                ItemModel.CountPermissions = ItemModel.ListPermission.Length;
            }

            return View(ModelListUserView);
        }

        public ActionResult GetOneGroup(long Id)
        {
        
            var GroupPermitionModel = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == Id).First();

            if (!GroupPermitionModel.Enabled)
            {
                ErrorMessage("Данная группа неактивна");
                return RedirectToAction("Group", "Permission");
            }

            ViewBag.UserList = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser_).Select(xxx => new ProducerInterfaceCommon.ContextModels.OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();

            ViewBag.PermissionList = cntx_.ControlPanelPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == SbyteTypeUser_).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();


            GroupPermitionModel.ListPermission = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == Id && xxx.TypeGroup == SbyteTypeUser_).First().ControlPanelPermission.Where(xxx => xxx.Enabled == true&& xxx.TypePermission == SbyteTypeUser_).Select(xxx => xxx.Id).ToList();
            GroupPermitionModel.ListUser = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == Id && xxx.TypeGroup == SbyteTypeUser_).First().ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser_).Select(xxx => xxx.Id).ToList();

            return View("ChangeGroupParameters", GroupPermitionModel);
        }

        [HttpPost]
        public ActionResult GroupChangeSave(ControlPanelGroup ChangedGroup)
        {
            if (ChangedGroup == null || ChangedGroup.Name == null || ChangedGroup.Name == "")
            {
                ErrorMessage("Не указано название группы");
                ViewBag.UserList = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser_).Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.PermissionList = cntx_.ControlPanelPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == SbyteTypeUser_).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
                return View("ChangeGroupParameters", ChangedGroup);
            }

            if (ChangedGroup.Id != 0)
            {
                // старая группа
                var GroupInDB = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == ChangedGroup.Id).First();

                GroupInDB.Name = ChangedGroup.Name;
                GroupInDB.Description = ChangedGroup.Description;
                GroupInDB.TypeGroup = SbyteTypeUser_;
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;


                // очищаем список пермишенов для данной группы
                var ListPermissionInDataBase = GroupInDB.ControlPanelPermission.ToList();

                foreach (var ListPermissionInDataBaseItem in ListPermissionInDataBase)
                {
                    GroupInDB.ControlPanelPermission.Remove(ListPermissionInDataBaseItem);
                }
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;

                // заполняем список пермишенов для данной группы  
                var NewListPermission = new List<ControlPanelPermission>();

                if (ChangedGroup.ListPermission != null) // если список не пуст, заполняем пермишены
                {
                    foreach (var OnePermisson in ChangedGroup.ListPermission)
                    {
                        NewListPermission.Add(cntx_.ControlPanelPermission.Where(xxx => xxx.Id == OnePermisson).First());
                    }
                    // вставляем в список пермишенов в группу
                    GroupInDB.ControlPanelPermission = NewListPermission;
                }

                //Очищаем список пользователей
                var ListUserInDateBase = GroupInDB.ProducerUser.ToList();

                foreach (var ListUserInDateBaseItem in ListUserInDateBase)
                {
                    GroupInDB.ProducerUser.Remove(ListUserInDateBaseItem);
                }
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;

                // заполняем список пользователей
                var NewListUsers = new List<ProducerUser>();
                if (ChangedGroup.ListUser != null)  // если список не пуст, заполняем пользователей
                {
                    foreach (var OnePermisson in ChangedGroup.ListUser)
                    {
                        NewListUsers.Add(cntx_.ProducerUser.Where(xxx => xxx.Id == OnePermisson).First());
                    }

                    // вставляем список пользователей
                    GroupInDB.ProducerUser = NewListUsers;
                }

                // сообщаем контексту об изменениях
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;

                //сохраняем
                cntx_.SaveChanges();
            }
            else
            {
                // новая группа
                var GroupInDB = new ControlPanelGroup();
                GroupInDB.Enabled = true;

                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Added;
                GroupInDB.Name = ChangedGroup.Name;
                GroupInDB.Description = ChangedGroup.Description;
                GroupInDB.TypeGroup = SbyteTypeUser_;
                cntx_.SaveChanges(); // сохраняем, что бы присвоился Id

                // заполняем список пермишенов для данной группы
                var NewListPermission = new List<ControlPanelPermission>();

                if (ChangedGroup.ListPermission != null) // если список не пуст, заполняем пермишены
                {
                    foreach (var OnePermisson in ChangedGroup.ListPermission)
                    {
                        NewListPermission.Add(cntx_.ControlPanelPermission.Where(xxx => xxx.Id == OnePermisson).First());
                    }
                    // вставляем в список пермишенов в группу
                    GroupInDB.ControlPanelPermission = NewListPermission;
                }

                // заполняем список пользователей
                var NewListUsers = new List<ProducerUser>();
                if (ChangedGroup.ListUser != null)  // если список не пуст, заполняем пользователей
                {
                    foreach (var OnePermisson in ChangedGroup.ListUser)
                    {
                        NewListUsers.Add(cntx_.ProducerUser.Where(xxx => xxx.Id == OnePermisson).First());
                    }

                    // вставляем список пользователей
                    GroupInDB.ProducerUser = NewListUsers;
                }

                // сообщаем контексту об изменениях
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;

                //сохраняем
                cntx_.SaveChanges();
            }

            SuccessMessage("Изменения в группе " + ChangedGroup.Name + " применены");
            return RedirectToAction("Group");
        }

        [HttpGet]
        public ActionResult CreateGroupPermission()
        {
            string Id = "";
            return View("CreateGroup", Id);
        }

        [HttpPost]
        public ActionResult CreateGroupPermission(string Id)
        {
            if (Id != null)
            {
                ViewBag.CreateName = Id;

                var GroupPermitionModel = new ControlPanelGroup();
                GroupPermitionModel.Name = Id;
                ViewBag.UserList = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser_).Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.PermissionList = cntx_.ControlPanelPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == SbyteTypeUser_).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
                GroupPermitionModel.ListPermission = new List<int>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => xxx.Id).ToList();
                GroupPermitionModel.ListUser = new List<long>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => xxx.Id).ToList();

                return View("ChangeGroupParameters", GroupPermitionModel);
            }
            ErrorMessage("Название группы Обязательный параметр");
            return View("CreateGroup", Id);
        }

    }
}