using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Helpers;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Validator.Constraints;
namespace ProducerInterface.Models
{
	/// <summary>
	///     Модель пользователя
	/// </summary>
	[Model(Database = "ProducerInterface")]
	public class ProducerUser : BaseModel
	{
		[Map, Description("ФИО"), ValidatorNotEmpty]
		public virtual string Name { get; set; }

		[Map, Description("Пароль"), ValidatorNotEmpty,
		 Length(Min = 5, Max = 20, Message = "Длина пароля должна быть не менее 5 и не более 20 символов.")]
		public virtual string Password { get; set; }

		[Map, Description("e-mail"), ValidatorNotEmpty, ValidatorEmail]
		public virtual string Email { get; set; }

		[Map, Description("Должность")]
		public virtual string Appointment { get; set; }

		[Map, Description("Время обновления пароля")]
		public virtual DateTime PasswordUpdated { get; set; }

		[Map, Description("Запрос на обновление пароля")]
		public virtual bool PasswordToUpdate { get; set; }

		[Map, Description("Заблокированный")]
		public virtual bool Enabled { get; set; }

		[HasMany(Table = "usertouserrole", ManyToMany = true)]
		public virtual IList<UserPermission> Permissions { get; set; }

		[BelongsTo]
		public virtual Producer Producer { get; set; }

		public virtual bool RegistrationIsAllowed(ISession dbSession, ref string message)
		{
			if (Producer == null || Producer.Id == 0) {
				message = "Укажите компанию изготовителя.";
				return false;
			}
			var noSimilarUsers = dbSession.Query<ProducerUser>().Any(s => s.Email == Email);
			if (noSimilarUsers) {
				message = "Пользователь с данной почтой уже зарегистрирован в системе.";
				return false;
			}
			return true;
		}

		/// <summary>
		///     Проверка прав
		/// </summary>
		/// <param name="permission">Права (если Null, возвращает true)</param>
		/// <returns>Подтверждение прав</returns>
		public virtual bool CheckPermission(UserPermission permission)
		{
			if (permission == null) {
				return true;
			}
			var permissionExists = GetUserPermissions().Any(s => s == permission);
			return permissionExists;
		}

		/// <summary>
		///     Получение всех прав пользователя
		/// </summary>
		/// <returns></returns>
		public virtual IList<UserPermission> GetUserPermissions()
		{
			var permissionsList = new List<UserPermission>();
			//	Roles.ForEach(s => permissionsList.AddRange(s.Permissions));
			permissionsList.AddRange(Permissions);
			return permissionsList;
		}

		/// <summary>
		///     Обновление пароля пользователя, замена на случайный
		/// </summary>
		/// <param name="dbSession"></param>
		public virtual string GetRandomPassword(ISession dbSession)
		{
			var dateBasePassword = "";
			if (!string.IsNullOrEmpty(Email)) {
				using (var md5Hash = MD5.Create()) {
					var dateBaseHash = Md5HashHelper.GetMd5Hash(md5Hash, SystemTime.Now().ToString());
					dateBasePassword = dateBaseHash.Substring(0, 6);
					Password = Md5HashHelper.GetMd5Hash(md5Hash, dateBasePassword);
				}
				PasswordToUpdate = true;
				PasswordUpdated = SystemTime.Now();
				dbSession.Save(this);
			}
			return dateBasePassword;
		}

		/// <summary>
		///     Обновление пароля пользователя, замена на случайный
		/// </summary>
		/// <param name="dbSession"></param>
		public virtual void UpdatePasswordByDefault(ISession dbSession)
		{
			if (!string.IsNullOrEmpty(Email)) {
				var dateBasePassword = GetRandomPassword(dbSession);
				// письмо пользователю
				EmailSender.SendEmail(Email, "Обновление данных авторизации", "Ваш новый пароль: " + dateBasePassword);
				// письмо письма на аналит
				EmailSender.SendEmail(GlobalConfig.GetParam("AnalitEmail"), "Обновление данных авторизации",
					"Ваш новый пароль: " + dateBasePassword);
			}
		}

		/// <summary>
		///     Обновление пароля пользователя, заманя на новый
		/// </summary>
		/// <param name="dbSession">Сесся БД</param>
		/// <param name="newPassword">Новый пароль</param>
		public virtual void UpdatePassword(ISession dbSession, string newPassword)
		{
			if (!string.IsNullOrEmpty(Email)) {
				PasswordUpdated = SystemTime.Now();
				Password = Md5HashHelper.GetHash(newPassword);
				PasswordUpdated = SystemTime.Now();
				dbSession.Save(this);
				// письмо пользователю
				EmailSender.SendEmail(Email, "Обновление данных авторизации", "Пароль был успешно изменен.");
				// письмо письма на аналит
				EmailSender.SendEmail(GlobalConfig.GetParam("AnalitEmail"), "Обновление данных авторизации",
					"Пароль был успешно изменен.");
			}
		}

		/// <summary>
		///     Обновление пароля пользователя, замена на случайный
		/// </summary>
		/// <param name="dbSession"></param>
		public virtual void ValidateUserPassword(string password)
		{
		}
	}
}