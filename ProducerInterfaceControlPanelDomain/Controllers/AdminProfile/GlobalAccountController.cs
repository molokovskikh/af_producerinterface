using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.GlobalAccount;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class GlobalAccountController : MasterBaseController
    {

        sbyte Type = (sbyte)ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser;

        // GET: GlobalAccount
        public ActionResult Index()
        {
            var ListUsers = cntx_.Account.Where(xxx=>xxx.TypeUser == Type).ToList();
            return View(ListUsers);
        }

        [HttpGet]
        public ActionResult SuccessAccount(long Id)
        {
            var ModelAccount = cntx_.Account.Where(xxx => xxx.Id == Id).First();
            ViewBag.Group = new List<long>();

            ViewBag.GroupList = cntx_.AccountGroup.Where(xxx => xxx.Enabled == true && xxx.TypeGroup == Type).ToList().Select(xxx => new OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name + "(" + xxx.Description + ")"}).ToList();


            return View(ModelAccount);            
        }

        [HttpPost]
        public ActionResult SuccessAccount(ProducerInterfaceCommon.ContextModels.Account userModel, List<long> Group)
        {
            var ModelAccount = cntx_.Account.Where(xxx => xxx.Id == userModel.Id).First();
            SuccessMessage("Пользователь добавлен, ему отправлено сообщение с паролем на почту");
            return View(ModelAccount);
        }

        [HttpGet]
        public ActionResult DeleteAccount(long Id)
        {
            SuccessMessage("Пока не реализовано");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult RegistrationCustomAccount(long Id)
        {
            var AccountModel = cntx_.Account.Find(Id);

            var ViewModel = cntx_.Account.Where(x => x.Id == Id).ToList().Select(x => new RegistrationCustomProfile
            {
                Id = x.Id,
                AppointmentId =(int) x.AppointmentId,
                CompanyName = cntx_.AccountCompany.Where(t => t.Id == x.AccountCompany.Id).First().Name,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Login = x.Login,
                OtherName = x.OtherName,
                Phone = x.Phone,
                SelectedGroupList = new List<int>(),
                CompanyId = x.AccountCompany.Id           
            }
            ).First();

            ViewBag.PostList = cntx_.AccountAppointment.Where(x => x.GlobalEnabled == 1 || x.Account.Any(z => z.Id == Id)).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();

            ViewBag.GroupList = cntx_.AccountGroup.Where(x => x.TypeGroup == (sbyte)TypeUsers.ProducerUser && x.Enabled == true).ToList().Select(x => new OptionElement
            {   Text = x.Name + " " + x.Description,
                Value = x.Id.ToString()
            }).ToList();

            return View("AccountVerification", ViewModel);

        }

        public ActionResult AccountVerification(RegistrationCustomProfile AccountModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PostList = cntx_.AccountAppointment.Where(x => x.GlobalEnabled == 1 || x.Account.Any(z => z.Id == AccountModel.Id)).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();

                ViewBag.GroupList = cntx_.AccountGroup.Where(x => x.TypeGroup == (sbyte)TypeUsers.ProducerUser && x.Enabled == true).ToList().Select(x => new OptionElement
                {
                    Text = x.Name + " " + x.Description,
                    Value = x.Id.ToString()
                }).ToList();

                return View(AccountModel);
            }

            string PassWord = "";
            var AccountModelSave = SaveAccount(AccountModel, ref PassWord);

            // отправка сообщения пользователю с паролем.

            ProducerInterfaceCommon.Heap.EmailSender.SendControlPanelRegistrationSuccessMessage(cntx_, AccountModelSave.Id, PassWord, CurrentUser.IP.ToString(), CurrentUser.Id);

            return RedirectToAction("RegistrationCustomAccount", new { Id = AccountModel.Id });

        }

        private Account SaveAccount(RegistrationCustomProfile AccountModel, ref string PassWord)
        {
            var AccountSave = cntx_.Account.Find(AccountModel.Id);

            AccountSave.PasswordUpdated = SystemTime.GetDefaultDate();
            PassWord = GetRandomPassword();
            AccountSave.Password = Md5HashHelper.GetHash(PassWord);

            AccountSave.Login = AccountModel.Login;
            AccountSave.Name = AccountModel.LastName + " " + AccountModel.FirstName + " " + AccountModel.OtherName;
            AccountSave.LastName = AccountModel.LastName;
            AccountSave.FirstName = AccountModel.FirstName;
            AccountSave.OtherName = AccountModel.OtherName;

            AccountSave.Phone = AccountModel.Phone;

            AccountSave.UserType =(sbyte) TypeUsers.ProducerUser;            
            AccountSave.AppointmentId = AccountModel.AppointmentId;
      
            cntx_.Entry(AccountSave).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();
            
            foreach (var GroupItem in AccountModel.SelectedGroupList)
            {
                AccountSave.AccountGroup.Add(cntx_.AccountGroup.Find(GroupItem));
            }

            cntx_.Entry(AccountSave).State = System.Data.Entity.EntityState.Modified;
         
            var AccountCompanyChange = cntx_.AccountCompany.Find(AccountModel.CompanyId);

            if (string.IsNullOrEmpty(AccountModel.CompanyName))
            {
                AccountCompanyChange.Name = "Физическое лицо";
            }
            else
            {
                AccountCompanyChange.Name = AccountModel.CompanyName;
            }

            cntx_.Entry(AccountCompanyChange).State = System.Data.Entity.EntityState.Modified;

            cntx_.SaveChanges();

            return AccountSave;
        }        
    }
}