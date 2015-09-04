using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using NHibernate.Mapping;

namespace ReportsControlPanel.Models
{
	public enum ContactGroupType
	{
		[Description("Общая информация")]
		General = 0,
		[Description("Администратор клиентов в автоматизированной системе")]
		ClientManagers = 1,
		[Description("Менеджер прайс листов(заказов)")]
		OrderManagers = 2,
		[Description("Бухгалтер по расчетам с АК \"Инфорум\"")]
		AccountantManagers = 3,
		[Description("Контактная информация для биллинга")]
		Billing = 4,
		[Description("Дополнительные контакты")]
		Custom = 5,
		[Description("Рассылка отчетов")]
		Reports = 6,
		[Description("Рассылка заказов")]
		OrdersDelivery = 7,
		[Description("Рассылка счетов")]
		Invoice = 8,
		[Description("Известные телефоны")]
		KnownPhones = 9,
		[Description("Список E-mail, с которых разрешена отправка писем клиентам АналитФармация")]
		MiniMails = 10,
		//Список контактов для подписки на отчеты
		[Description("Самостоятельная подписка на отчеты")]
		ReportSubscribers = 11,
		[Description("Список адресов, на которые не будет отправлять письма спаморезка")]
		MiniMailNoSend = 12
	}

	[Model("contact_groups","contacts")]
	public class ContactGroup : BaseModel
	{
		[Map(PrimaryKey = true)]
		public virtual uint Id { get; set; }

		[Map]
		public virtual string Name { get; set; }

		[Map]
		public virtual ContactGroupType Type { get; set; }

		[Map]
		public virtual bool Public { get; set; }

		[Map]
		public virtual bool Specialized { get; set; }

		[HasMany(Column = "ContactOwnerId")]
		public virtual IList<Contact> Contacts { get; set; }
	}
}