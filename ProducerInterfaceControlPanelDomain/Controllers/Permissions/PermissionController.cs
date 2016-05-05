using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Permission;

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
                    CountUser = xxx.Account.Where(eee => eee.Enabled == (sbyte)UserStatus.Active && eee.TypeUser == FilterSbyte).Count(),
                    NameGroup = xxx.Name,
                    Description = xxx.Description,
                    Users = xxx.Account.Where(zzz => zzz.TypeUser == FilterSbyte && zzz.Enabled == (sbyte)UserStatus.Active && zzz.TypeUser == FilterSbyte).Select(zzz => zzz.Name + "-" + zzz.Login).ToArray(),
                    Permissions = xxx.AccountPermission.Where(zzz => zzz.Enabled == true && zzz.TypePermission == FilterSbyte).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray()
                });

            return View(ListGroup);
        }

        [HttpGet]
        public ActionResult ChangeAccount(long Id)
        {
            var AccountModel = cntx_.Account.Where(x => x.Id == Id).ToList().Select(x =>
            new AdminAccountValidation
            {
                Id = x.Id,
                Name = x.Name,
                TypeUser = x.TypeUser ,
                RegionMask = x.RegionMask,
                ListGroup = x.AccountGroup.ToList()
            }).First();

            if (AccountModel.TypeUser != (sbyte)ProducerInterfaceCommon.ContextModels.TypeUsers.ControlPanelUser)
            {
                ErrorMessage("Выбранный пользователь не является администратором");
                return RedirectToAction("Index", "Home");
            }
                      

         

            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            
            ViewBag.RegionList = h.GetRegionList();
            AccountModel.RegionListId = h.GetRegionList((decimal)AccountModel.RegionMask).ToList().Select(x => Convert.ToInt64(x.Value)).ToList();

            ViewBag.GroupList = cntx_.AccountGroup.Where(x => x.Enabled == true && x.TypeGroup == (sbyte)TypeUsers.ControlPanelUser)
                .ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
            AccountModel.GroupListId = AccountModel.ListGroup.ToList().Select(x => x.Id).ToList();
            
            return View(AccountModel);
        }

        [HttpPost]
        public ActionResult ChangeAccount(AdminAccountValidation AccountModel)
        {

            if (!ModelState.IsValid)
            {
                ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);

                ViewBag.RegionList = h.GetRegionList();

                ViewBag.GroupList = cntx_.AccountGroup.Where(x => x.Enabled == true && x.TypeGroup == (sbyte)TypeUsers.ControlPanelUser)
                        .ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();

                return View(AccountModel);
            }

            var ListGroup = cntx_.AccountGroup.Where(x => x.Account.Any(t => t.Id == AccountModel.Id)).ToList();

            foreach (var GroupItem in AccountModel.GroupListId)
            {
                if (!ListGroup.Any(x => x.Id == GroupItem))
                {
                    // добавляем пользователя в группу

                    var Group = cntx_.AccountGroup.Find(GroupItem);
                    Group.Account.Add(cntx_.Account.Find(AccountModel.Id));
                    cntx_.Entry(Group).State = System.Data.Entity.EntityState.Modified;
                    cntx_.SaveChanges();
                }             
            }

            foreach (var GroupItem in ListGroup)
            {
                if (AccountModel.GroupListId.Where(x => x == GroupItem.Id).Count() == 0)
                {
                    var Group = cntx_.AccountGroup.Find(GroupItem.Id);
                    Group.Account.Remove(cntx_.Account.Find(AccountModel.Id));
                    cntx_.Entry(Group).State = System.Data.Entity.EntityState.Modified;
                    cntx_.SaveChanges();
                }
            }        
                 
            SaveAccountRegionMask(AccountModel.Id, AccountModel.RegionListId);

            SuccessMessage("Изменения сохранены");
            return RedirectToAction("ListUsers");
        }
        
        private ulong ConvertLongListToRegionMask(List<long> RegionList)
        {
            ulong RegionMask = 0;

            foreach (var RegionItem in RegionList)
            {
                if (RegionMask == 0)
                {
                    RegionMask = (ulong)RegionItem;
                }
                else
                {
                    RegionMask = (ulong)RegionMask & (ulong)RegionItem;
                }
            }

            return RegionMask;
        }
              
        public void SaveAccountRegionMask(long IdAccount, List<long> RegionList)
        {
            var Account_ = cntx_.Account.Find(IdAccount);
            
            cntx_.Account.Attach(Account_);
            var entry = cntx_.Entry(Account_);

            Account_.RegionMask = ConvertLongListToRegionMask(RegionList);

            entry.Property(e => e.RegionMask).IsModified = true;                      
            cntx_.SaveChanges();        
        }


        public ActionResult ListUsers()
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id, this.HttpContext);       
            var AccountList = cntx_.Account.Where(X => X.TypeUser == (sbyte)TypeUsers.ControlPanelUser).ToList();

            foreach (var AccountItem in AccountList)
            {
                AccountItem.ListPermission = h.GetRegionList((decimal)AccountItem.RegionMask);
            }
            return View(AccountList);
        }

    }
}