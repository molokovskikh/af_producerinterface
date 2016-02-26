using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class BasePermissionController : MasterBaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        public TypeUsers FilterType
        {
            get
            {
                return (ProducerInterfaceCommon.ContextModels.TypeUsers)FilterSbyte;
            }
            set
            {
                FilterSbyte = (SByte)value;
            }
        }

        protected SByte FilterSbyte { get; set; }

        public ActionResult Index()
        {
            string AdministrationGroupname = GetWebConfigParameters("AdminGroupName");

            var ListAdministrators = cntx_.AccountGroup.Where(xxx => xxx.Name == AdministrationGroupname && xxx.TypeGroup == FilterSbyte).First().Account.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == FilterSbyte).ToList();
            return View(ListAdministrators);
        }
        
                 
        public ActionResult Users()
        {
            var ModelListUserView = cntx_.Account.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == FilterSbyte).ToList()
                .Select(vvv => new ListUserView
                {
                    Id = vvv.Id,
                    Name = vvv.Name,
                    Groups = cntx_.AccountGroup.ToList().Where(zzz => zzz.Account.Any(zzzz => zzzz.Id == vvv.Id && zzzz.TypeUser == FilterSbyte)).ToList().Select(z => z.Name).ToArray(),
                    ListPermission = cntx_.AccountPermission.ToList().Where(zzz => zzz.AccountGroup.Any(ccc => ccc.Account.Any(v => v.Id == vvv.Id && v.TypeUser == FilterSbyte))).ToList().Select(vv => vv.ControllerAction + "   " + vv.ActionAttributes).ToArray()
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

            var GroupPermitionModel = cntx_.AccountGroup.Where(xxx => xxx.Id == Id).First();

            if (!GroupPermitionModel.Enabled)
            {
                ErrorMessage("Данная группа неактивна");
                return RedirectToAction("Group", "Permission");
            }

            ViewBag.UserList = cntx_.Account.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == FilterSbyte).Select(xxx => new ProducerInterfaceCommon.ContextModels.OptionElement { Text = xxx.Name + " - " +  xxx.Login, Value = xxx.Id.ToString() }).ToList();

            ViewBag.PermissionList = cntx_.AccountPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == FilterSbyte).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();


            GroupPermitionModel.ListPermission = cntx_.AccountGroup.Where(xxx => xxx.Id == Id && xxx.TypeGroup == FilterSbyte).First().AccountPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == FilterSbyte).Select(xxx => xxx.Id).ToList();
            GroupPermitionModel.ListUser = cntx_.AccountGroup.Where(xxx => xxx.Id == Id && xxx.TypeGroup == FilterSbyte).First().Account.Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == FilterSbyte).Select(xxx => xxx.Id).ToList();

            return View("ChangeGroupParameters", GroupPermitionModel);
        }

        [HttpPost]
        public ActionResult GroupChangeSave(AccountGroup ChangedGroup)
        {
            if (ChangedGroup == null || ChangedGroup.Name == null || ChangedGroup.Name == "")
            {
                ErrorMessage("Не указано название группы");
                ViewBag.UserList = cntx_.Account.Where(xxx => xxx.Enabled == 1 && xxx.Login != null && xxx.TypeUser == FilterSbyte).Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.PermissionList = cntx_.AccountPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == FilterSbyte).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
                return View("ChangeGroupParameters", ChangedGroup);
            }

            if (ChangedGroup.Id != 0)
            {
                // старая группа
                var GroupInDB = cntx_.AccountGroup.Where(xxx => xxx.Id == ChangedGroup.Id).First();

                GroupInDB.Name = ChangedGroup.Name;
                GroupInDB.Description = ChangedGroup.Description;
                GroupInDB.TypeGroup = FilterSbyte;
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;

                // обновляем список пермишенов для данной группы              
                List<int> ListIntPermission = GroupInDB.AccountPermission.ToList().Select(x => x.Id).ToList();
                
                ChangePermissionListInGroup(GroupInDB.Id, ListIntPermission, ChangedGroup.ListPermission);

                //обновляем список пользователей для данной группы

                var oldListAccount = GroupInDB.Account.ToList().Select(x => x.Id).ToList();
                
                ChangeAccountListInGroup(GroupInDB.Id, oldListAccount, ChangedGroup.ListUser);
                
            }
            else
            {
                // новая группа
                var GroupInDB = new AccountGroup();
                GroupInDB.Enabled = true;

                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Added;
                GroupInDB.Name = ChangedGroup.Name;
                GroupInDB.Description = ChangedGroup.Description;
                GroupInDB.TypeGroup = FilterSbyte;
                cntx_.SaveChanges(); // сохраняем, что бы присвоился Id
                   
                // заполняем список доступов для группы       
                if (ChangedGroup.ListPermission != null) // если список не пуст, заполняем пермишены
                {
                    ChangePermissionListInGroup(GroupInDB.Id, new List<int>(), ChangedGroup.ListPermission);                    
                }

                // заполняем список пользователей в данной группе         
                if (ChangedGroup.ListUser != null)  // если список не пуст, заполняем пользователей
                {
                    ChangeAccountListInGroup(GroupInDB.Id, new List<long>(), ChangedGroup.ListUser);                 
                }
            
            }

            SuccessMessage("Изменения в группе " + ChangedGroup.Name + " применены");
            return RedirectToAction("Group");
        }




        private void ChangeAccountListInGroup(int IdGroup, List<long> OldListUser, List<long> NewListUser)
        {

            var GroupItem = cntx_.AccountGroup.Find(IdGroup);
            bool ExsistSummary = true;

            if (OldListUser != null && NewListUser != null)
            {
                bool AccountExsistListNew = NewListUser.Any(x => !OldListUser.Contains(x));
                bool AccountExsistListOld = OldListUser.Any(x => !NewListUser.Contains(x));

                // получаем false если список пользователей не изменился
                ExsistSummary = (AccountExsistListNew || AccountExsistListOld);
            }
            if (ExsistSummary)
            {
                if (NewListUser == null || NewListUser.Count() == 0)
                {
                    GroupItem.Account = new List<Account>();
                }
                else
                {
                    GroupItem.Account = new List<Account>();
                    cntx_.Entry(GroupItem).State = System.Data.Entity.EntityState.Modified;
                    cntx_.SaveChanges();
                    GroupItem.Account = cntx_.Account.Where(x => NewListUser.Contains(x.Id)).ToList();                 
                }
                cntx_.Entry(GroupItem).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges();
            }
        }

        private void ChangePermissionListInGroup(int IdGroup, List<int> OldListPermission, List<int> NewListPermission)
        {
            var GroupItem = cntx_.AccountGroup.Find(IdGroup);         
            bool ExsistSummary = true;
            if (OldListPermission != null && NewListPermission != null)
            {
                bool PermissionExsistListNew = NewListPermission.Any(x => !OldListPermission.Contains(x));
                bool PermissionExsistListOld = OldListPermission.Any(x => !NewListPermission.Contains(x));

                // получаем false если список доступов не изменился
                ExsistSummary = (PermissionExsistListNew || PermissionExsistListOld);
            }      

            if (ExsistSummary)
            {
                if (NewListPermission == null || NewListPermission.Count() == 0)
                {

                    var ListOldPermission = cntx_.AccountPermission.Where(x => OldListPermission.Contains(x.Id)).ToList();

                    foreach (var RemovieItem in ListOldPermission)
                    {
                        GroupItem.AccountPermission.Remove(RemovieItem);
                        cntx_.Entry(GroupItem).State = System.Data.Entity.EntityState.Modified;
                        cntx_.SaveChanges();
                    }     
                }
                else
                {
                    var ListOldPermission = cntx_.AccountPermission.Where(x => OldListPermission.Contains(x.Id)).ToList();

                    foreach (var RemovieItem in ListOldPermission)
                    {
                        GroupItem.AccountPermission.Remove(RemovieItem);
                        cntx_.Entry(GroupItem).State = System.Data.Entity.EntityState.Modified;
                        cntx_.SaveChanges();
                    }

                    GroupItem.AccountPermission = cntx_.AccountPermission.Where(x => NewListPermission.Contains(x.Id)).ToList();
                    cntx_.SaveChanges();
                }                         
            }            
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

                var GroupPermitionModel = new AccountGroup();
                GroupPermitionModel.Name = Id;
                ViewBag.UserList = cntx_.Account.Where(xxx => xxx.Enabled == 1 && xxx.Login != null && xxx.TypeUser == FilterSbyte).Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.PermissionList = cntx_.AccountPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == FilterSbyte).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
                GroupPermitionModel.ListPermission = new List<int>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => xxx.Id).ToList();
                GroupPermitionModel.ListUser = new List<long>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => xxx.Id).ToList();

                return View("ChangeGroupParameters", GroupPermitionModel);
            }
            ErrorMessage("Название группы Обязательный параметр");
            return View("CreateGroup", Id);
        }

        [HttpGet]
        public ActionResult ListPermission()
        {
            var ListPermission = cntx_.AccountPermission.Where(xxx => xxx.TypePermission == FilterSbyte).ToList();
            return View(ListPermission);
        }

        [HttpGet]
        public ActionResult EditPermission(int Id = 0)
        {
            var ListPermission = cntx_.AccountPermission.Where(xxx => xxx.Id == Id).First();

            return View(ListPermission);
        }

        [HttpPost]
        public ActionResult EditPermission(AccountPermission CPP_)
        {

            var PermissionItem = cntx_.AccountPermission.Where(xxx => xxx.Id == CPP_.Id).First();

            PermissionItem.Description = CPP_.Description;
            cntx_.Entry(PermissionItem).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();


            SuccessMessage("Описание сохранено");
            return RedirectToAction("ListPermission");
        }
    }
}