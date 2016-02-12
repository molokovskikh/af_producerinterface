using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using Quartz;
using Quartz.Impl;
using ProducerInterfaceCommon.Heap;
using System.Configuration;
using System.Collections.Specialized;

namespace ProducerInterface.Controllers
{
    public class ProfileController : MasterBaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.BreadCrumb = "Профиль пользователя";
        }

        public ActionResult Index()
        {
            ViewBag.News = cntx_.NotificationToProducers.Take(10).ToList();
            return View();
        }

        [HttpGet]
        public ActionResult Account()
        {

            var ThisUser = cntx_.Account.Where(xxx => xxx.Id == CurrentUser.Id).First();

            var ModelView = new ProducerInterfaceCommon.ContextModels.ProfileValidation();

            ModelView.AppointmentId = ThisUser.AccountAppointment.Id;
            ModelView.CompanyName = cntx_.producernames.Where(xxx => xxx.ProducerId == ThisUser.AccountCompany.ProducerId).First().ProducerName;
            ModelView.EmailDomain = ThisUser.AccountCompany.CompanyDomainName.Where(xxx => ThisUser.Login.Contains(xxx.Name)).First().Id;
            ModelView.Mailname = ThisUser.Login.Split(new Char[] { '@' }, StringSplitOptions.RemoveEmptyEntries)[0].ToString();
            ModelView.Name = ThisUser.Name;
            ModelView.PhoneNumber = ThisUser.Phone;

            var AppointmentList =
             cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                 .ToList()
                 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                 .ToList();

            var UserOptionAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == ThisUser.AccountAppointment.Id).ToList().Select(xxx => new OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name }).First();

            if (!AppointmentList.Contains(UserOptionAppointment))
            {
                AppointmentList.Add(UserOptionAppointment);
            }

            ViewBag.AppointmentList = AppointmentList;
            ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();

            return View(ModelView);
        }

        [HttpPost]
        public ActionResult Account(ProducerInterfaceCommon.ContextModels.ProfileValidation changeProfile)
        {

            if (!ModelState.IsValid)
            {

                var AppointmentList =
                 cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
                     .ToList()
                     .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
                     .ToList();

                var UserOptionAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == CurrentUser.AccountAppointment.Id).ToList().Select(xxx => new OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name }).First();

                if (!AppointmentList.Contains(UserOptionAppointment))
                {
                    AppointmentList.Add(UserOptionAppointment);
                }

                ViewBag.AppointmentList = AppointmentList;
                ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();

                return View(changeProfile);
            }

            var AccountSave = cntx_.Account.Where(xxx => xxx.Id == CurrentUser.Id).First();

            AccountSave.Name = changeProfile.Name;
            AccountSave.Login = changeProfile.Mailname + "@" + AccountSave.AccountCompany.CompanyDomainName.Where(xxx => xxx.Id == changeProfile.EmailDomain).First().Name;
            AccountSave.Phone = changeProfile.PhoneNumber;
            AccountSave.AccountAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == changeProfile.AppointmentId).First();

            cntx_.Entry(AccountSave).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Изменения сохранены");
            return RedirectToAction("Index");
        }

        public ActionResult GetOldNews(int Pages)
        {
            var News = cntx_.NotificationToProducers.Skip(Pages * 10).Take(10).ToList();
            return PartialView(News);
        }

        public ActionResult GetNextList(int Pager)
        {
            ViewBag.Pager = Pager + 1;
            var ListNews10 = cntx_.NotificationToProducers.OrderBy(xxx => xxx.Id).Take(10).ToList();
            return PartialView("GetNextList", ListNews10);
        }

    }
}

