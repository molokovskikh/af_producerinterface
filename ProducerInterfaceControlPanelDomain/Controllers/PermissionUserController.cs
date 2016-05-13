using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Permission;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class PermissionUserController : MasterBaseController
	{

		/// <summary>
		/// Список групп пользователей
		/// </summary>
		/// <returns></returns>
		public ActionResult Group()
		{
			var ListGroup = cntx_.AccountGroup.Where(xxx => xxx.TypeGroup == (sbyte)TypeUsers.ProducerUser).ToList()
					.Select(xxx => new ListGroupView
					{
						Id = xxx.Id,
						ListUsersInGroup = cntx_.Account.Where(zzz => zzz.AccountGroup.Any(vvv => vvv.Id == xxx.Id)).ToList().Select(d => new UsersViewInChange { eMail = d.Login, Name = d.Name, ProducerName = d.AccountCompany.Name }).ToList(),
						CountUser = xxx.Account.Where(eee => eee.Enabled == (sbyte)UserStatus.Active && eee.TypeUser == (sbyte)TypeUsers.ProducerUser).Count(),
						NameGroup = xxx.Name,
						Description = xxx.Description,
						Users = xxx.Account.Where(zzz => zzz.TypeUser == (sbyte)TypeUsers.ProducerUser && zzz.Enabled == (sbyte)UserStatus.Active && zzz.TypeUser == (sbyte)TypeUsers.ProducerUser).Select(zzz => zzz.Name).ToArray(),
						Permissions = xxx.AccountPermission.Where(zzz => zzz.Enabled == true && zzz.TypePermission == (sbyte)TypeUsers.ProducerUser).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray()
					});

			return View(ListGroup);
		}


		/// <summary>
		/// Редактирование пользователя GET
		/// </summary>
		/// <param name="Id">идентификатор пользователя</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Change(long Id)
		{
			var user = cntx_.Account.Single(x => x.Id == Id);
			var model = new UserEdit()
			{
				Name = user.Name,
				Status = user.Enabled,
				AppointmentId = user.AppointmentId,
				AccountGroupIds = user.AccountGroup.Select(x => x.Id).ToList()
			};

			// если запрос на регистрацию - показываем сообщение от пользователя
			if (user.EnabledEnum == UserStatus.Request)
				model.Message = user.AccountCompany.Name;

			SetChangeModel(user, model);
			return View(model);
		}


		/// <summary>
		/// Редактирование пользователя POST
		/// </summary>
		/// <param name="model">модель правок пользователя</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Change(UserEdit model)
		{
			var user = cntx_.Account.Single(x => x.Id == model.Id);
			if (!ModelState.IsValid)
			{
				SetChangeModel(user, model);
				return View(model);
			}

			var groups = cntx_.AccountGroup.Where(x => model.AccountGroupIds.Contains(x.Id));
			user.AccountGroup.Clear();
			foreach (var group in groups)
				user.AccountGroup.Add(group);

			var sendAcceptMail = false;
			var password = "";
			// если подтверждение регистрации пользователя
			if (user.EnabledEnum == UserStatus.Request && model.Status == (sbyte)UserStatus.New) {
				password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				user.PasswordUpdated = DateTime.Now;
				sendAcceptMail = true;
			}

			user.AppointmentId = model.AppointmentId;
			user.Enabled = model.Status;
			user.LastUpdatePermisison = DateTime.Now;
			cntx_.SaveChanges();

			// отправка сообщения пользователю с паролем
			if (sendAcceptMail)
				EmailSender.SendAccountVerificationMessage(cntx_, user, password, CurrentUser.Id);

			SuccessMessage("Изменения успешно сохранены");
			return RedirectToAction("Index");
		}

		private void SetChangeModel(Account user, UserEdit model)
		{
			model.AllAccountGroup = cntx_.AccountGroup.Where(x => x.TypeGroup == (sbyte)TypeUsers.ProducerUser)
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			// очередность смены статусов
			var allStatus = new List<SelectListItem>();
			switch (user.EnabledEnum) {
					case UserStatus.Active: // активный можно изменить только на заблокированный
					allStatus.Add(new SelectListItem() { Text = UserStatus.Active.DisplayName(), Value = ((int)UserStatus.Active).ToString(), Selected = model.Status == (sbyte)UserStatus.Active });
					allStatus.Add(new SelectListItem() { Text = UserStatus.Blocked.DisplayName(), Value = ((int)UserStatus.Blocked).ToString(), Selected = model.Status == (sbyte)UserStatus.Blocked });
					break;
				case UserStatus.Blocked: // заблокированный можно изменить только на активный
					allStatus.Add(new SelectListItem() { Text = UserStatus.Blocked.DisplayName(), Value = ((int)UserStatus.Blocked).ToString(), Selected = model.Status == (sbyte)UserStatus.Blocked });
					allStatus.Add(new SelectListItem() { Text = UserStatus.Active.DisplayName(), Value = ((int)UserStatus.Active).ToString(), Selected = model.Status == (sbyte)UserStatus.Active });
					break;
				case UserStatus.New: // новый нельзя изменить
					allStatus.Add(new SelectListItem() { Text = UserStatus.New.DisplayName(), Value = ((int)UserStatus.New).ToString(), Selected = model.Status == (sbyte)UserStatus.New });
					break;
				case UserStatus.Request: // запрос регистрации можно изменить только на новый
					allStatus.Add(new SelectListItem() { Text = UserStatus.Request.DisplayName(), Value = ((int)UserStatus.Request).ToString(), Selected = model.Status == (sbyte)UserStatus.Request });
					allStatus.Add(new SelectListItem() { Text = UserStatus.New.DisplayName(), Value = ((int)UserStatus.New).ToString(), Selected = model.Status == (sbyte)UserStatus.New });
					break;
			}
			model.AllStatus = allStatus;

			// должности
			var allAppointment = new List<SelectListItem>();
			if (!user.AppointmentId.HasValue)
				allAppointment.Add(new SelectListItem() { Text = "", Value = "", Selected = true });
			allAppointment.AddRange(cntx_.AccountAppointment.OrderBy(x => x.Name)
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == user.AppointmentId })
				.ToList());

			model.AllAppointment = allAppointment;
		}

		/// <summary>
		/// Поиск пользователя
		/// </summary>
		/// <param name="filter">фильтр поиска</param>
		/// <returns></returns>
		public ActionResult SearchUser(UserFilter filter)
		{
			var query = cntx_.Account.Where(x => x.TypeUser == (sbyte)filter.TypeUserEnum);
			if (filter.UserId.HasValue)
				query = query.Where(x => x.Id == filter.UserId.Value);
			if (!string.IsNullOrEmpty(filter.UserName))
				query = query.Where(x => x.Name.Contains(filter.UserName));
			if (!string.IsNullOrEmpty(filter.Login))
				query = query.Where(x => x.Login.Contains(filter.Login));
			if (!string.IsNullOrEmpty(filter.ProducerName))
			{
				var producerIds = cntx_.producernames.Where(x => x.ProducerName.Contains(filter.ProducerName)).Select(x => x.ProducerId).ToList();
				query = query.Where(x => x.AccountCompany.ProducerId.HasValue && producerIds.Contains(x.AccountCompany.ProducerId.Value));
			}
			if (filter.AccountGroupId.HasValue)
				query = query.Where(x => x.AccountGroup.Any(y => y.Id == filter.AccountGroupId.Value));
			if (filter.AppointmentId.HasValue)
			{
				switch (filter.AppointmentId.Value)
				{
					case 0: // без должности
						query = query.Where(x => x.AccountAppointment == null);
						break;
					case -1: // с индивидуальной должностью
						query = query.Where(x => !x.AccountAppointment.GlobalEnabled);
						break;
					default:
						query = query.Where(x => x.AppointmentId == filter.AppointmentId.Value);
						break;
				}
			}
			if (filter.Status.HasValue)
				query = query.Where(x => x.Enabled == filter.Status.Value);

			// установка пейджера
			var itemsCount = query.Count();
			var itemsPerPage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
			var info = new SortingPagingInfo() { CurrentPageIndex = filter.CurrentPageIndex, ItemsCount = itemsCount, ItemsPerPage = itemsPerPage };
			ViewBag.Info = info;

			// имена производителей
			var existProducerIds = cntx_.AccountCompany.Where(x => x.ProducerId.HasValue).Select(x => x.ProducerId).Distinct().ToList();
			ViewBag.Producers = cntx_.producernames.Where(x => existProducerIds.Contains(x.ProducerId)).ToDictionary(x => x.ProducerId, x => x.ProducerName);

			var model = query.OrderByDescending(x => x.Id).Skip(filter.CurrentPageIndex * itemsPerPage).Take(itemsPerPage).ToList();
			return View(model);
		}

		/// <summary>
		/// Список пользователей сайта
		/// </summary>
		/// <returns></returns>
		public ActionResult Index(UserFilter model)
		{
			var emptyElement = new SelectListItem() { Text = "", Value = "" };

			// список групп
			var allAccountGroup = new List<SelectListItem>() { emptyElement };
			allAccountGroup.AddRange(cntx_.AccountGroup
				.Where(x => x.Enabled && x.TypeGroup == (sbyte)TypeUsers.ProducerUser)
				.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString(), Selected = model.AccountGroupId == x.Id })
				.ToList());
			model.AllAccountGroup = allAccountGroup;

			// список статусов
			var allStatus = new List<SelectListItem>() { emptyElement };
			if (model.Status.HasValue)
				allStatus.AddRange(EnumHelper.GetSelectList(typeof(UserStatus), (UserStatus)model.Status.Value));
			else
				allStatus.AddRange(EnumHelper.GetSelectList(typeof(UserStatus)));
			model.AllStatus = allStatus;

			// список должностей
			var allAppointment = new List<SelectListItem>() { emptyElement };
			allAppointment.Add(new SelectListItem() { Text = "Без должности", Value = "0", Selected = model.AppointmentId == 0 });
			allAppointment.Add(new SelectListItem() { Text = "С индивидуальной должностью", Value = "-1", Selected = model.AppointmentId == -1 });
			allAppointment.AddRange(cntx_.AccountAppointment
				.Where(x => x.Account.Any())
				.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString(), Selected = model.AppointmentId == x.Id })
				.ToList());
			model.AllAppointment = allAppointment;

			return View(model);
		}

		/// <summary>
		/// Форма редактирования свойств группы пользователей GET
		/// </summary>
		/// <param name="Id">идентификатор группы</param>
		/// <returns></returns>
		public ActionResult GetOneGroup(long Id)
		{
			var accountGroup = cntx_.AccountGroup.Single(x => x.Id == Id);
			if (!accountGroup.Enabled)
			{
				ErrorMessage("Данная группа неактивна");
				return RedirectToAction("Group", "PermissionUser");
			}

			// все пользователи
			ViewBag.UserList = cntx_.Account.Where(x => x.Enabled == (sbyte)UserStatus.Active && x.TypeUser == (sbyte)TypeUsers.ProducerUser).Select(x => new OptionElement { Text = x.Name + " - " + x.Login, Value = x.Id.ToString() }).ToList();
			// все права
			ViewBag.PermissionList = cntx_.AccountPermission.Where(x => x.Enabled && x.TypePermission == (sbyte)TypeUsers.ProducerUser).Select(x => new OptionElement { Text = x.ControllerAction + " " + x.Description, Value = x.Id.ToString() }).ToList();

			accountGroup.ListPermission = accountGroup.AccountPermission.Select(x => x.Id).ToList();
			accountGroup.ListUser = accountGroup.Account.Select(x => x.Id).ToList();

			return View("ChangeGroupParameters", accountGroup);
		}

		/// <summary>
		/// Сохранение изменений свойств группы пользователей POST
		/// </summary>
		/// <param name="ChangedGroup"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GroupChangeSave(AccountGroup ChangedGroup)
		{
			if (ChangedGroup == null || ChangedGroup.Name == null || ChangedGroup.Name == "")
			{
				ErrorMessage("Не указано название группы");
				ViewBag.UserList = cntx_.Account.Where(xxx => xxx.Enabled == (sbyte)UserStatus.Active && xxx.Login != null && xxx.TypeUser == (sbyte)TypeUsers.ProducerUser).Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
				ViewBag.PermissionList = cntx_.AccountPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == (sbyte)TypeUsers.ProducerUser).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
				return View("ChangeGroupParameters", ChangedGroup);
			}

			if (ChangedGroup.Id != 0)
			{
				// старая группа
				var GroupInDB = cntx_.AccountGroup.Where(xxx => xxx.Id == ChangedGroup.Id).First();

				GroupInDB.Name = ChangedGroup.Name;
				GroupInDB.Description = ChangedGroup.Description;
				GroupInDB.TypeGroup = (sbyte)TypeUsers.ProducerUser;
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
				GroupInDB.TypeGroup = (sbyte)TypeUsers.ProducerUser;
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
				ViewBag.UserList = cntx_.Account.Where(xxx => xxx.Enabled == (sbyte)UserStatus.Active && xxx.Login != null && xxx.TypeUser == (sbyte)TypeUsers.ProducerUser).Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() }).ToList();
				ViewBag.PermissionList = cntx_.AccountPermission.Where(xxx => xxx.Enabled == true && xxx.TypePermission == (sbyte)TypeUsers.ProducerUser).Select(xxx => new OptionElement { Text = xxx.ControllerAction + " " + xxx.Description, Value = xxx.Id.ToString() }).ToList();
				GroupPermitionModel.ListPermission = new List<int>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelPermission.Where(xxx => xxx.Enabled == true).Select(xxx => xxx.Id).ToList();
				GroupPermitionModel.ListUser = new List<long>(); // cntx_.ControlPanelGroup.Where(xxx => xxx.Id == 0).First().ControlPanelUser.Where(xxx => xxx.Enabled == 1).Select(xxx => xxx.Id).ToList();

				return View("ChangeGroupParameters", GroupPermitionModel);
			}
			ErrorMessage("Название группы Обязательный параметр");
			return View("CreateGroup", Id);
		}

		/// <summary>
		/// Список прав
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ListPermission()
		{
			var ListPermission = cntx_.AccountPermission.Where(xxx => xxx.TypePermission == (sbyte)TypeUsers.ProducerUser).OrderBy(x => x.ControllerAction).ToList();
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

		public ActionResult DeletePermission(int Id = 0)
		{
			if (Id != 0)
			{
				var permissionItem = cntx_.AccountPermission.Where(x => x.Id == Id).First();

				var GroupList = cntx_.AccountGroup.Where(x => x.AccountPermission.Any(z => z.Id == permissionItem.Id)).ToList();

				foreach (var GroupItem in GroupList)
				{
					GroupItem.AccountPermission.Remove(permissionItem);
				}
				cntx_.SaveChanges();

				cntx_.AccountPermission.Remove(permissionItem);
				cntx_.SaveChanges();

				SuccessMessage("Доступ удален, если доступ данный есть то он будет снова добавлен при следующем открытии страницы. И автоматически добавится в группу 'Администраторы'");
				return RedirectToAction("ListPermission");
			}

			return RedirectToAction("ListPermission");
		}
	}
}