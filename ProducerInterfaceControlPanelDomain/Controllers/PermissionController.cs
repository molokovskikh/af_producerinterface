﻿using System;
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
            
        public ActionResult Group()
        {
            var ListGroup = cntx_.ControlPanelGroup.ToList()
                .Select(xxx => new Models.ListGroupView {
                    Id = xxx.Id,
                    CountUser = xxx.ControlPanelUser.Where(eee => eee.Enabled == 1).Count(),
                    NameGroup = xxx.Name,
                    Description = xxx.Description,
                    Users = xxx.ControlPanelUser.Where(zzz => zzz.Enabled == 1).Select(zzz => zzz.Name).ToArray(),
                    Permissions = xxx.ControlPanelPermission.Where(zzz => zzz.Enabled == true).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray()
                });

            return View(ListGroup);
        }

        public ActionResult Users()
        {

            var ListUserViewModel = cntx_.controlpaneluserpermission.ToList()
                .GroupBy(xxx=>xxx.IdUser).Select(xxx=>xxx.First())         
                .Select(
                xxx => new Models.ListUserView
                {
                    Id = (long)xxx.IdUser,
                    Name = xxx.Name,
                    Groups = cntx_.controlpaneluserpermission.Where(yyy => yyy.IdUser == xxx.IdUser)
                            .Select(eee => eee.GroupName).Distinct()
                            .ToArray(),

                    CountGroup = cntx_.controlpaneluserpermission.Where(yyy => yyy.IdUser == xxx.IdUser)
                            .Select(eee => eee.GroupName).Distinct()
                            .Count(),

                    CountPermissions = cntx_.controlpaneluserpermission.Where(yyy => yyy.IdUser == xxx.IdUser)
                            .Select(yyy => yyy.ControllerAction + "  " + yyy.ActionAttributes).Distinct().Count(),

                    ListPermission = cntx_.controlpaneluserpermission.Where(yyy => yyy.IdUser == xxx.IdUser)
                            .Select(yyy => yyy.ControllerAction + "  " + yyy.ActionAttributes).Distinct().ToArray()
                }
                ).ToList();
            return View(ListUserViewModel);
        }

        public ActionResult GetOneGroup(long Id)
        {
            var GroupPermitionModel = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == Id).First();

            if (!GroupPermitionModel.Enabled)
            {
                ErrorMessage("Данная группа неактивна");
                return RedirectToAction("Group", "Permission");
            }

            ViewBag.UserList = cntx_.ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => new Models.OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();

            ViewBag.PermissionList = cntx_.ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => new Models.OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
               

            GroupPermitionModel.ListPermission = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == Id).First().ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => xxx.Id).ToList();
            GroupPermitionModel.ListUser = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == Id).First().ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => xxx.Id).ToList();

            return View("ChangeGroupParameters", GroupPermitionModel);
        }

        [HttpPost]
        public ActionResult GroupChangeSave(Models.ControlPanelGroup ChangedGroup)
        {
            if (ChangedGroup == null || ChangedGroup.Name == null || ChangedGroup.Name == "")
            {
                ErrorMessage("Не указано название группы");
                ViewBag.UserList = cntx_.ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => new Models.OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.PermissionList = cntx_.ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => new Models.OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
                return View("ChangeGroupParameters", ChangedGroup);
            }

            if (ChangedGroup.Id != 0)
            {
                // старая группа
                var GroupInDB = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == ChangedGroup.Id).First();

                GroupInDB.Name = ChangedGroup.Name;
                GroupInDB.Description = ChangedGroup.Description;
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;
          
                
                // очищаем список пермишенов для данной группы
                var ListPermissionInDataBase = GroupInDB.ControlPanelPermission.ToList();

                foreach (var ListPermissionInDataBaseItem in ListPermissionInDataBase)
                {
                    GroupInDB.ControlPanelPermission.Remove(ListPermissionInDataBaseItem);
                }
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;
                
                // заполняем список пермишенов для данной группы  
                var NewListPermission = new List<Models.ControlPanelPermission>();

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
                var ListUserInDateBase = GroupInDB.ControlPanelUser.ToList();

                foreach (var ListUserInDateBaseItem in ListUserInDateBase)
                {
                    GroupInDB.ControlPanelUser.Remove(ListUserInDateBaseItem);
                }
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;                

                // заполняем список пользователей
                var NewListUsers = new List<Models.ControlPanelUser>();
                if (ChangedGroup.ListUser != null)  // если список не пуст, заполняем пользователей
                {
                    foreach (var OnePermisson in ChangedGroup.ListUser)
                    {
                        NewListUsers.Add(cntx_.ControlPanelUser.Where(xxx => xxx.Id == OnePermisson).First());
                    }

                    // вставляем список пользователей
                    GroupInDB.ControlPanelUser = NewListUsers;
                }           
                
                // сообщаем контексту об изменениях
                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Modified;

                //сохраняем
                cntx_.SaveChanges();
            }
            else
            {
                // новая группа
                var GroupInDB =new Models.ControlPanelGroup();
                GroupInDB.Enabled = true;

                cntx_.Entry(GroupInDB).State = System.Data.Entity.EntityState.Added;    
                GroupInDB.Name = ChangedGroup.Name;
                GroupInDB.Description = ChangedGroup.Description;
                cntx_.SaveChanges(); // сохраняем, что бы присвоился Id

                // заполняем список пермишенов для данной группы
                var NewListPermission = new List<Models.ControlPanelPermission>();

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
                var NewListUsers = new List<Models.ControlPanelUser>();
                if (ChangedGroup.ListUser != null)  // если список не пуст, заполняем пользователей
                {
                    foreach (var OnePermisson in ChangedGroup.ListUser)
                    {
                        NewListUsers.Add(cntx_.ControlPanelUser.Where(xxx => xxx.Id == OnePermisson).First());
                    }

                    // вставляем список пользователей
                    GroupInDB.ControlPanelUser = NewListUsers;
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

                var GroupPermitionModel = new Models.ControlPanelGroup();
                GroupPermitionModel.Name = Id;
                ViewBag.UserList = cntx_.ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => new Models.OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
                ViewBag.PermissionList = cntx_.ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => new Models.OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
                GroupPermitionModel.ListPermission = new List<long>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => xxx.Id).ToList();
                GroupPermitionModel.ListUser = new List<long>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => xxx.Id).ToList();

                return View("ChangeGroupParameters", GroupPermitionModel);
            }
            ErrorMessage("Название группы Обязательный параметр");
            return View("CreateGroup", Id);
        }



    }
}