using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Permission;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class PermissionUserController : BaseController
	{

		/// <summary>
		/// Список групп пользователей
		/// </summary>
		/// <returns></returns>
		public ActionResult Group()
		{
			var model = DB.AccountGroup.ToList()
					.Select(x => new ListGroupView
					{
						Id = x.Id,
						NameGroup = x.Name,
						TypeUser = (TypeUsers)x.TypeGroup,
						Description = x.Description,
						ListUsersInGroup = x.Account.Select(y => new UsersViewInChange
						{
							eMail = y.Login,
							Name = y.Name,
							ProducerName = y.AccountCompany != null ? y.AccountCompany.Name : "" // у админов нет компании
						}).ToList(),
						Permissions = x.AccountPermission
							.Where(y => y.Enabled)
							.Select(y => y.ControllerAction + " " + y.ActionAttributes)
							.OrderBy(y => y)
							.ToArray()
					});

			return View(model);
		}


		/// <summary>
		/// Редактирование пользователя GET
		/// </summary>
		/// <param name="Id">идентификатор пользователя</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Change(long Id)
		{
			var user = DB.Account.Single(x => x.Id == Id);
			var model = new UserEdit()
			{
				Name = user.Name,
				Status = user.Enabled,
				AppointmentId = user.AppointmentId,
				AccountGroupIds = user.AccountGroup.Select(x => x.Id).ToList(),
				AccountRegionIds = user.AccountRegion.Select(x => x.RegionCode).ToList()
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
			var user = DB.Account.Single(x => x.Id == model.Id);
			if (!ModelState.IsValid)
			{
				SetChangeModel(user, model);
				return View(model);
			}

			var groups = DB.AccountGroup.Where(x => model.AccountGroupIds.Contains(x.Id));
			user.AccountGroup.Clear();
			foreach (var group in groups)
				user.AccountGroup.Add(group);

			user.AccountRegion.Clear();
			foreach (var regionCode in model.AccountRegionIds)
				user.AccountRegion.Add(new AccountRegion() { AccountId = user.Id, RegionCode = regionCode });

			var sendAcceptMail = false;
			var password = "";
			// если подтверждение регистрации пользователя
			if (user.EnabledEnum == UserStatus.Request && model.Status == (sbyte)UserStatus.New)
			{
				password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				user.PasswordUpdated = DateTime.Now;
				sendAcceptMail = true;
			}

			user.AppointmentId = model.AppointmentId;
			user.Enabled = model.Status;
			user.LastUpdatePermisison = DateTime.Now;
			DB.SaveChanges();

			// отправка сообщения пользователю с паролем
			if (sendAcceptMail)
				EmailSender.SendAccountVerificationMessage(DB, user, password, CurrentUser.Id);

			SuccessMessage("Изменения успешно сохранены");
			// если админ - на список админов
			if (user.TypeUser == (sbyte)TypeUsers.ControlPanelUser)
				return RedirectToAction("AdminList");

			return RedirectToAction("Index");
		}

		private void SetChangeModel(Account user, UserEdit model)
		{
			model.AllAccountGroup = DB.AccountGroup.Where(x => x.TypeGroup == user.TypeUser)
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			// очередность смены статусов
			var allStatus = new List<SelectListItem>();
			switch (user.EnabledEnum)
			{
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
			allAppointment.AddRange(DB.AccountAppointment.OrderBy(x => x.Name)
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == user.AppointmentId })
				.ToList());
			model.AllAppointment = allAppointment;

			var regions = DB.regionsnamesleaf
				.ToList()
				.OrderBy(x => x.RegionName)
				.Select(x => new SelectListItem { Value = x.RegionCode.ToString(), Text = x.RegionName, Selected = model.AccountRegionIds.Contains(x.RegionCode) })
				.ToList();
			model.AllAccountRegion = regions;
		}

		/// <summary>
		/// Поиск пользователя
		/// </summary>
		/// <param name="filter">фильтр поиска</param>
		/// <returns></returns>
		public ActionResult SearchUser(UserFilter filter)
		{
			var query = DB.Account.Where(x => x.TypeUser == (sbyte)filter.TypeUserEnum);
			if (!string.IsNullOrEmpty(filter.UserName))
				query = query.Where(x => x.Name.Contains(filter.UserName));
			if (!string.IsNullOrEmpty(filter.Login))
				query = query.Where(x => x.Login.Contains(filter.Login));
			if (!string.IsNullOrEmpty(filter.Phone))
				query = query.Where(x => x.Phone.Contains(filter.Phone));
			if (!string.IsNullOrEmpty(filter.ProducerName))
			{
				var producerIds = DB.producernames.Where(x => x.ProducerName.Contains(filter.ProducerName)).Select(x => x.ProducerId).ToList();
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
			var existProducerIds = DB.AccountCompany.Where(x => x.ProducerId.HasValue).Select(x => x.ProducerId).Distinct().ToList();
			ViewBag.Producers = DB.producernames.Where(x => existProducerIds.Contains(x.ProducerId)).ToDictionary(x => x.ProducerId, x => x.ProducerName);

			var model = query.OrderByDescending(x => x.Id).Skip(filter.CurrentPageIndex * itemsPerPage).Take(itemsPerPage).ToList();
			return View(model);
		}

		/// <summary>
		/// Список админов
		/// </summary>
		/// <returns></returns>
		public ActionResult AdminList()
		{
			var model = DB.Account.Where(x => x.TypeUser == (sbyte)TypeUsers.ControlPanelUser).OrderByDescending(x => x.Id).ToList();
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
			allAccountGroup.AddRange(DB.AccountGroup
				.Where(x => x.Enabled && x.TypeGroup == (sbyte)model.TypeUserEnum)
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
			allAppointment.AddRange(DB.AccountAppointment
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
			var model = DB.AccountGroup.Single(x => x.Id == Id);

			if (!model.Enabled)
			{
				ErrorMessage("Данная группа неактивна");
				return RedirectToAction("Group", "PermissionUser");
			}

			// все пользователи
			ViewBag.UserList = DB.Account.Where(x => x.Enabled == (sbyte)UserStatus.Active && x.TypeUser == model.TypeGroup).Select(x => new OptionElement { Text = x.Name + " - " + x.Login, Value = x.Id.ToString() }).ToList();
			// все права
			ViewBag.PermissionList = DB.AccountPermission
				.Where(x => x.Enabled && x.TypePermission == model.TypeGroup)
				.OrderBy(x => new { x.ControllerAction, x.ActionAttributes })
				.Select(x => new OptionElement { Text = x.ControllerAction + " " + x.ActionAttributes, Value = x.Id.ToString() })
				.ToList();

			model.ListPermission = model.AccountPermission.Select(x => x.Id).ToList();
			model.ListUser = model.Account.Select(x => x.Id).ToList();

			return View("ChangeGroupParameters", model);
		}

		/// <summary>
		/// Сохранение изменений свойств группы пользователей POST
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GroupChangeSave(AccountGroup model)
		{
			if (string.IsNullOrEmpty(model.Name))
			{
				ErrorMessage("Не указано название группы");
				ViewBag.UserList = DB.Account.Where(x => x.Enabled == (sbyte)UserStatus.Active && x.TypeUser == model.TypeGroup).Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
				ViewBag.PermissionList = DB.AccountPermission
					.Where(x => x.Enabled && x.TypePermission == model.TypeGroup)
					.OrderBy(x => new { x.ControllerAction, x.ActionAttributes })
					.Select(x => new OptionElement { Text = x.ControllerAction + " " + x.ActionAttributes, Value = x.Id.ToString() })
					.ToList();
				return View("ChangeGroupParameters", model);
			}

			var group = DB.AccountGroup.SingleOrDefault(x => x.Id == model.Id);
			if (group == null)
			{
				group = new AccountGroup() { Enabled = true, TypeGroup = model.TypeGroup, Name = model.Name };
				DB.AccountGroup.Add(group);
				DB.SaveChanges();
			}
			group.Name = model.Name;
			group.Description = model.Description;
			DB.SaveChanges();

			// если очищено всё
			if (model.ListPermission == null)
				model.ListPermission = new List<int>();

			// обновляем список пермишенов для данной группы
			var groupPermissions = DB.AccountPermission.Where(x => model.ListPermission.Contains(x.Id)).ToList();
			group.AccountPermission.Clear();
			foreach (var permission in groupPermissions)
				group.AccountPermission.Add(permission);

			// если очищено всё
			if (model.ListUser == null)
				model.ListUser = new List<long>();

			// пользователи, что более не состоят в данной группе
			var deletedAccounts = group.Account.Where(x => !model.ListUser.Contains(x.Id)).ToList();
			foreach (var account in deletedAccounts)
				account.LastUpdatePermisison = DateTime.Now;

			//обновляем список пользователей для данной группы
			var groupAccounts = DB.Account.Where(x => model.ListUser.Contains(x.Id)).ToList();
			group.Account.Clear();
			foreach (var account in groupAccounts) {
				group.Account.Add(account);
				account.LastUpdatePermisison = DateTime.Now;
			}
			DB.SaveChanges();

			SuccessMessage("Изменения в группе " + model.Name + " сохранены");
			return RedirectToAction("Group");
		}

		/// <summary>
		/// Добавление группы GET
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CreateGroup()
		{
			var model = new AccountGroup() { TypeGroup = (sbyte)TypeUsers.ProducerUser };
			return View(model);
		}

		/// <summary>
		/// Добавление группы POST
		/// </summary>
		/// <param name="model">Модель группы, где заполнено имя и тип</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CreateGroup(AccountGroup model)
		{
			if (string.IsNullOrEmpty(model.Name))
			{
				ModelState.AddModelError("Name", "Укажите название группы");
				return View(model);
			}
			model.ListPermission = new List<int>();
			model.ListUser = new List<long>();

			ViewBag.UserList = DB.Account.Where(x => x.Enabled == (sbyte)UserStatus.Active && x.TypeUser == model.TypeGroup).Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
			ViewBag.PermissionList = DB.AccountPermission
				.Where(x => x.Enabled && x.TypePermission == model.TypeGroup)
				.OrderBy(x => new { x.ControllerAction, x.ActionAttributes })
				.Select(x => new OptionElement { Text = x.ControllerAction + " " + x.ActionAttributes, Value = x.Id.ToString() })
				.ToList();

			return View("ChangeGroupParameters", model);
		}

		/// <summary>
		/// Список прав
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ListPermission()
		{
			var model = DB.AccountPermission.OrderBy(x => new { x.TypePermission, x.ControllerAction}).ToList();
			return View(model);
		}

		/// <summary>
		/// Редактирование права доступа GET
		/// </summary>
		/// <param name="Id">идентификатор права доступа</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditPermission(int Id = 0)
		{
			var model = DB.AccountPermission.Single(x => x.Id == Id);
			return View(model);
		}

		/// <summary>
		/// Редактирование права доступа POST
		/// </summary>
		/// <param name="model">заполненная модель права доступа</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditPermission(AccountPermission model)
		{
			var permission = DB.AccountPermission.Single(x => x.Id == model.Id);
			permission.Description = model.Description;
			DB.SaveChanges();

			SuccessMessage("Описание сохранено");
			return RedirectToAction("ListPermission");
		}

		/// <summary>
		/// Удаление права доступа
		/// </summary>
		/// <param name="Id">идентификатор права доступа</param>
		/// <returns></returns>
		public ActionResult DeletePermission(int Id = 0)
		{
			var permission = DB.AccountPermission.SingleOrDefault(x => x.Id == Id);
			if (permission == null) {
				ErrorMessage("Доступ не найден");
				return RedirectToAction("ListPermission");
			}

			permission.AccountGroup.Clear();
			DB.SaveChanges();

			DB.AccountPermission.Remove(permission);
			DB.SaveChanges();

			SuccessMessage("Доступ удален, но если он есть на сайте, то он будет снова добавлен при следующем открытии страницы и добавится в группу 'Администраторы'");
			return RedirectToAction("ListPermission");
		}
	}
}