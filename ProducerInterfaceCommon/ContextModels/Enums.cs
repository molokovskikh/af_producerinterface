using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ContextModels
{

	public enum Operation
	{
		[Display(Name = "Добавление")]
		Insert = 0,

		[Display(Name = "Изменение")]
		Update = 1,

		[Display(Name = "Удаление")]
		Delete = 2
	}

	public enum CatalogLogType
	{
		[Display(Name = "Описание")]
		Descriptions = 1,

		[Display(Name = "МНН")]
		MNN = 2,

		[Display(Name = "ПКУ")]
		PKU = 3
	}

	public enum DisplayStatus
	{

		[Display(Name = "Не запускался")]
		New = 0,

		[Display(Name = "Отчет готовится")]
		Processed = 1,

		[Display(Name = "Посмотреть отчет")]
		Ready = 2,

		[Display(Name = "Нет данных для построения отчета")]
		Empty = 3,

		[Display(Name = "В процессе подготовки произошла ошибка")]
		Error = 4
	}

	public enum FeedBackType // UI FeedBackTypePrivate - нумерация пунктов должна совпадать (UI пользовательский интерфейс)
	{
		[Display(Name = "Пожелание")]
		Wish = 0,
		[Display(Name = "Ошибка")]
		Error = 1,
		[Display(Name = "Другое")]
		Other = 2,
		[Display(Name = "Добавление должности")]
		AddNewAppointment = 3,
		/*
				как минимум 4й - 5й пункт  "4" должен отсутствовать        
		*/

	}

	public enum FeedBackTypePrivate // FeedBackType - нумерация должна совпадать   (UI админка)
	{
		[Display(Name = "Пожелание")]
		Wish = 0,
		[Display(Name = "Ошибка")]
		Error = 1,
		[Display(Name = "Другое")]
		Other = 2,
		[Display(Name = "Добавление должности")]
		AddNewAppointment = 3,
		//[Display(Name = "Заявка на Регистрацию")]
		//Registration = 4,
		[Display(Name = "Добавление домена")]
		AddNewDomainName = 5
	}

	public enum FeedBackStatus
	{
		[Display(Name = "Новое обращение")]
		New = 0,

		[Display(Name = "Обработано")]
		Processed = 1
	}

	public enum NewsChanges
	{
		[Display(Name = "Добавление")]
		NewsAdd = 0,
		[Display(Name = "Редактирование")]
		NewsChange = 1,
		[Display(Name = "Отправлено в архив")]
		NewsArchive = 2,
		[Display(Name = "Возвращено из архива")]
		NewsRetArchive = 3
	}

	public enum Reports
	{
		[Display(Name = "Рейтинг товаров")]
		ProductRatingReport = 1,

		[Display(Name = "Рейтинг аптек")]
		PharmacyRatingReport = 2,

		[Display(Name = "Рейтинг поставщиков")]
		SupplierRatingReport = 3,

		[Display(Name = "Рейтинг товаров в конкурентной группе")]
		ProductConcurentRatingReport = 4,

		[Display(Name = "Динамика цен и остатков по товару за период")]
		ProductPriceDynamicsReport = 5,

		[Display(Name = "Продажи вторичных дистрибьюторов")]
		SecondarySalesReport = 6,

		[Display(Name = "Мониторинг остатков у дистрибьюторов")]
		ProductResidueReport = 7
	}

	public enum CatalogVar
	{
		[Display(Name = "всему ассортименту")]
		AllAssortiment = 1,

		[Display(Name = "всем нашим товарам")]
		AllCatalog = 2,

		[Display(Name = "выбранным товарам")]
		SelectedProducts = 3,
	}

	public enum CostOrQuantity
	{
		[Display(Name = "цену и остаток")]
		WithCostAndQuantity = 1,

		[Display(Name = "только цену")]
		WithCost = 2,

		[Display(Name = "только остаток")]
		WithQuantity = 3,
	}

	public enum TypeUsers
	{
		[Display(Name = "Пользователь из Интерфейса производителя")]
		ProducerUser = 0,
		[Display(Name = "Пользователь из Панели управления")]
		ControlPanelUser = 1,
		//[Display(Name = "Пользователь из интерфейса производителя, без Id производителя")]
		//UserNotProducer = 2
	}

	public enum UserStatus
	{
		[Display(Name = "Новый")]
		New = 0,
		[Display(Name = "Активный")]
		Active = 1,
		[Display(Name = "Заблокированный")]
		Blocked = 2,
		[Display(Name = "Запрос регистрации")]
		Request = 3
	}


	public enum EntityCommand
	{
		[Display(Name = "Добавлено")]
		Added = 4,

		[Display(Name = "Удалено")]
		Deleted = 8,

		[Display(Name = "Изменено")]
		Modified = 16
	}

	public enum PromotionStatus
	{
		[Display(Name = "Не подтверждена")]
		СonfirmedFalse = 0,
		[Display(Name = "Ожидание даты начала ")]
		ConfirmedNotBegin = 1,
		[Display(Name = "Завершена")]
		ConfirmedEnd = 2,
		[Display(Name = "Публикуется")]
		Confirmed = 3,
		[Display(Name = "Не публикуется")]
		ConfirmedNotView = 4,
		[Display(Name = "Любой")]
		All = 5
	}

	public enum EntityType
	{
		[Display(Name = "Новости")]
		News = 1,
		[Display(Name = "Акции")]
		Promotion = 2,
		[Display(Name = "Рассылка")]
		Email = 3
	}

	public enum PromotionTypeChange
	{
		[Display(Name = "Добавлено")]
		AddPromo = 0,
		[Display(Name = "Удалено")]
		DeletePromo = 1,
		[Display(Name = "Изменено")]
		ModifiedPromo = 2,
		[Display(Name = "Подтверждена/Отменена")]
		Modified_Activate = 3
	}

}
