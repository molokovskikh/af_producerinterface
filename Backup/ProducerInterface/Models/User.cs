using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using Analit.Components;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Helpers;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Mapping.Attributes;
using NHibernate.Validator.Constraints;

namespace ProducerInterface.Models
{
	/// <summary>
	/// Модель пользователя
	/// </summary>
	[Class( Table = "Users", NameType = typeof(User), Schema = "producerinterface")]
	public class User : BaseModel
	{
		[Property, Description("ФИО"), NotNullNotEmpty(Message = "Поле должно быть заполненно.")]
		public virtual string Name { get; set; }

		[Property, Description("Логин"), NotNullNotEmpty(Message = "Поле должно быть заполненно."),
		 Length(Min = 4, Max = 20, Message = "Длина логина должна быть не менее 5 и не более 20 символов.")]
		public virtual string Login { get; set; }

		[Property, Description("Пароль"), NotNullNotEmpty(Message = "Поле должно быть заполненно."),
		 Length(Min = 5, Max = 20, Message = "Длина пароля должна быть не менее 5 и не более 20 символов.")]
		public virtual string Password { get; set; }

		[Property, Description("e-mail"), NotNullNotEmpty(Message = "Поле должно быть заполненно."), ValidatorEmail]
		public virtual string Email { get; set; }

		[Property, Description("Должность")]
		public virtual string Appointment { get; set; }

		[Property, Description("Время обновления пароля")]
		public virtual DateTime PasswordUpdated { get; set; }

		[Property, Description("Запрос на обновление пароля")]
		public virtual bool PasswordToUpdate { get; set; }

		[Property, Description("Заблокированный")]
		public virtual bool Enabled { get; set; }

		[ManyToOne]
		public virtual Producer Producer { get; set; }

		public virtual bool RegistrationIsAllowed(ISession dbSession, ref string message)
		{
			if (Login == null || Login.Length < 5 || Login.Length > 20) {
				message = "Длина логина должна быть не менее 5 и не более 20 символов.";
				return false;
			}
			if (Producer == null || Producer.Id == 0) {
				message = "Укажите компанию изготовителя.";
				return false;
			}
			var noSimilarUsers = dbSession.Query<User>().Any(s => s.Login == Login || s.Email == Email);
			if (noSimilarUsers) {
				message = "Пользователь с подобным логином или почтой уже зарегистрирован в системе.";
				return false;
			}
			return true;
		}

		/// <summary>
		/// Обновление пароля пользователя, замена на случайный
		/// </summary>
		/// <param name="dbSession"></param>
		public virtual string GetRandomPassword(ISession dbSession)
		{
			string dateBasePassword = "";
			if (!string.IsNullOrEmpty(Email)) {
				using (MD5 md5Hash = MD5.Create()) {
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
		/// Обновление пароля пользователя, замена на случайный
		/// </summary>
		/// <param name="dbSession"></param>
		public virtual void UpdatePasswordByDefault(ISession dbSession)
		{
			if (!string.IsNullOrEmpty(Email)) {
				string dateBasePassword = GetRandomPassword(dbSession);
				// письмо пользователю
				EmailSender.SendEmail(Email, "Обновление данных авторизации", "Ваш новый пароль: " + dateBasePassword);
				// письмо письма на аналит
				EmailSender.SendEmail(Config.GetParam("AnalitEmail"), "Обновление данных авторизации", "Ваш новый пароль: " + dateBasePassword);
			}
		}


		/// <summary>
		/// Обновление пароля пользователя, заманя на новый
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
				EmailSender.SendEmail(Config.GetParam("AnalitEmail"), "Обновление данных авторизации", "Пароль был успешно изменен.");
			}
		}

		/// <summary>
		/// Обновление пароля пользователя, замена на случайный
		/// </summary>
		/// <param name="dbSession"></param>
		public virtual void ValidateUserPassword(string password)
		{
		}
	}
}