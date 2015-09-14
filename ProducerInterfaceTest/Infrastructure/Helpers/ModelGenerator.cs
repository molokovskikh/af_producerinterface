using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analit.Components;
using AnalitFramefork.Hibernate.Models;
using NHibernate;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterfaceTest.Infrastructure.Helpers
{
	public class ModelGenerator
	{
		private ISession DbSession { get; set; }

		/// <summary>
		/// Автогенерация необходимых для тестов моделей
		/// </summary>
		/// <param name="dbSession"></param>
		public ModelGenerator(ISession dbSession)
		{
			DbSession = dbSession;
			// в строго заданном порядке
			Generate_Producers();
			Generate_ProducerUsers();
			Generate_UserPermissions();
			Generate_DrugDescription();
			Generate_DrugDescriptionRemark();
			Generate_MNN();
			Generate_DrugFamily();
			Generate_DrugForm();
			Generate_Drug();
		}

		////////////////////////////////////|  ProducerInterface  |//////////////////////////////////////////

		//Producer
		public Producer Clone_Producers(Producer producer = null)
		{
			if (producer == null) {
				DbSession.Flush();
				producer = DbSession.Query<Producer>().FirstOrDefault();
				if (producer == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста", typeof (Producer).Name));
				}
			}
			var cloneProducer = new Producer();
			cloneProducer.Name = producer.Name;
			DbSession.Save(cloneProducer);
			return cloneProducer;
		}

		public void Generate_Producers()
		{
			//_1
			var cloneProducer = new Producer();
			cloneProducer.Name = "A&S PHARMACEUTICAL";
			DbSession.Save(cloneProducer);
			//_2
			cloneProducer = new Producer();
			cloneProducer.Name = "AARTI DRUGS LTD.";
			DbSession.Save(cloneProducer);
			//_3
			cloneProducer = new Producer();
			cloneProducer.Name = "ABBOTT LABORATORIES";
			DbSession.Save(cloneProducer);

			DbSession.Flush();
		}

		//ProducerUser
		public ProducerUser Clone_ProducerUsers(ProducerUser producerUser = null)
		{
			if (producerUser == null) {
				DbSession.Flush();
				producerUser = DbSession.Query<ProducerUser>().FirstOrDefault();
				if (producerUser == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста", typeof (ProducerUser).Name));
				}
			}
			var cloneProducerUser = new ProducerUser();
			cloneProducerUser.Email = producerUser.Email;
			cloneProducerUser.Name = producerUser.Name;
			cloneProducerUser.Appointment = producerUser.Appointment;
			cloneProducerUser.Enabled = producerUser.Enabled;
			cloneProducerUser.Password = producerUser.Password;
			cloneProducerUser.PasswordToUpdate = producerUser.PasswordToUpdate;
			cloneProducerUser.PasswordUpdated = producerUser.PasswordUpdated;
			cloneProducerUser.Permissions = producerUser.Permissions;
			cloneProducerUser.Producer = producerUser.Producer;
			return cloneProducerUser;
		}

		public void Generate_ProducerUsers()
		{
			//_1
			var cloneProducerUser = new ProducerUser();
			cloneProducerUser.Email = "newUser@mail.ru";
			cloneProducerUser.Name = "Вася Пупкин";
			cloneProducerUser.Appointment = "Просто Юзер";
			cloneProducerUser.Enabled = true;
			cloneProducerUser.Password = "какой-то хэш";
			cloneProducerUser.PasswordToUpdate = false;
			cloneProducerUser.PasswordUpdated = SystemTime.Now();
			cloneProducerUser.Permissions = new List<UserPermission>();
			cloneProducerUser.Producer = DbSession.Query<Producer>().FirstOrDefault();
			DbSession.Save(cloneProducerUser);
			//_2 
			cloneProducerUser = Clone_ProducerUsers(cloneProducerUser);
			cloneProducerUser.Email = "secondUser@mail.ru";
			cloneProducerUser.Name = "Степан Иванов";
			cloneProducerUser.Permissions = new List<UserPermission>();
			cloneProducerUser.Producer = DbSession.Query<Producer>().FirstOrDefault();
			DbSession.Save(cloneProducerUser);
			DbSession.Flush();
		}

		//UserPermission
		public UserPermission Clone_UserPermissions(UserPermission userPermissions = null)
		{
			if (userPermissions == null) {
				DbSession.Flush();
				userPermissions = DbSession.Query<UserPermission>().FirstOrDefault();
				if (userPermissions == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (UserPermission).Name));
				}
			}
			var cloneProducer = new UserPermission();
			cloneProducer.Name = userPermissions.Name;
			cloneProducer.Description = userPermissions.Description;
			cloneProducer.Users = userPermissions.Users;
			return cloneProducer;
		}

		public void Generate_UserPermissions()
		{
			//_1
			var cloneProducer = new UserPermission();
			cloneProducer.Name = "controller_action";
			cloneProducer.Description = "Право хранить молчание";
			cloneProducer.Users = new List<ProducerUser>();
			DbSession.Save(cloneProducer);
			DbSession.Flush();
		}

		//DrugDescriptionRemark
		public DrugDescriptionRemark Clone_DrugDescriptionRemark(DrugDescriptionRemark element = null)
		{
			if (element == null) {
				DbSession.Flush();
				element = DbSession.Query<DrugDescriptionRemark>().FirstOrDefault();
				if (element == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (DrugDescriptionRemark).Name));
				}
			}
			var newElement = new DrugDescriptionRemark();
			newElement.Name = element.Name;
			newElement.CreationDate = element.CreationDate;
			newElement.Composition = element.Composition;
			newElement.Dosing = element.Dosing;
			newElement.EnglishName = element.EnglishName;
			newElement.Expiration = element.Expiration;
			newElement.IndicationsForUse = element.IndicationsForUse;
			newElement.Interaction = element.Interaction;
			newElement.PharmacologicalAction = element.PharmacologicalAction;
			newElement.ProductForm = element.ProductForm;
			newElement.Status = element.Status;
			newElement.Description = element.Description;
			newElement.SideEffect = element.SideEffect;
			newElement.EnglishName = element.EnglishName;
			newElement.Storage = element.Storage;
			newElement.Warnings = element.Warnings;
			newElement.Modificator = element.Modificator;
			newElement.ModificationDate = element.ModificationDate;
			return element;
		}

		public void Generate_DrugDescriptionRemark()
		{
			//_1
			var element = new DrugDescriptionRemark();
			element.Name = "New DrugFamily";
			element.CreationDate = SystemTime.Now();
			element.Composition = "";
			element.Dosing = "";
			element.EnglishName = "";
			element.Expiration = "";
			element.IndicationsForUse = "";
			element.Interaction = "";
			element.PharmacologicalAction = "";
			element.ProductForm = "";
			element.Status = DrugDescriptionRemarkStatus.New;
			element.Description = "";
			element.SideEffect = "";
			element.EnglishName = "";
			element.Storage = "";
			element.Warnings = "";
			element.Modificator = DbSession.Query<Admin>().FirstOrDefault();
			element.ModificationDate = SystemTime.Now();
			DbSession.Save(element);
			DbSession.Flush();
		}

		//DrugDescription
		public DrugDescription Clone_DrugDescription(DrugDescription element = null)
		{
			if (element == null) {
				DbSession.Flush();
				element = DbSession.Query<DrugDescription>().FirstOrDefault();
				if (element == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (DrugDescription).Name));
				}
			}
			var newElement = new DrugDescription();
			newElement.Name = element.Name;
			newElement.UpdateTime = element.UpdateTime;
			newElement.Composition = element.Composition;
			newElement.Dosing = element.Dosing;
			newElement.EnglishName = element.EnglishName;
			newElement.Expiration = element.Expiration;
			newElement.IndicationsForUse = element.IndicationsForUse;
			newElement.Interaction = element.Interaction;
			newElement.PharmacologicalAction = element.PharmacologicalAction;
			newElement.ProductForm = element.ProductForm;
			newElement.ShouldBeCorrected = element.ShouldBeCorrected;
			newElement.Description = element.Description;
			newElement.SideEffect = element.SideEffect;
			newElement.EnglishName = element.EnglishName;
			newElement.Storage = element.Storage;
			newElement.Warnings = element.Warnings;
			return element;
		}

		public void Generate_DrugDescription()
		{
			//_1
			var element = new DrugDescription();
			element.Name = "100 СИЛ (БАД)";
			element.UpdateTime = SystemTime.Now();
			element.Composition = "";
			element.Dosing = "";
			element.EnglishName = "";
			element.Expiration = "";
			element.IndicationsForUse = "";
			element.Interaction = "";
			element.PharmacologicalAction = "";
			element.ProductForm = "";
			element.ShouldBeCorrected = true;
			element.Description = "";
			element.SideEffect = "";
			element.EnglishName = "";
			element.Storage = "";
			element.Warnings = "";
			DbSession.Save(element);
			DbSession.Flush();
			//_2
			element = new DrugDescription();
			element.Name = "120-80";
			element.UpdateTime = SystemTime.Now();
			element.Composition = @"120/80 таблетки по 0,5 г содержат: 
витамины C, D, B1, B2, B6, B12, PP, A, E, магний, калий, боярышник, МКЦ";
			element.Dosing = "по 1 таблетке 3 раза в день во время еды с пищей.";
			element.EnglishName = "";
			element.Expiration = "2 года";
			element.IndicationsForUse = "Рекомендован в качестве БАД  к пище, дополнительного источника витаминов и магния.";
			element.Interaction = "Нет сведений";
			element.PharmacologicalAction =
				"120/80 - БАД к пище, питательный комплекс для поддержания сердечной деятельности. Магний повышает тонус сердечной мышцы. Калий осуществляет активную доставку питательных веществ в клетки, поддерживает вводно-солевой баланс, восстанавливает нормальное функционирование клеток. Боярышник регулирует давление и содержание холестерина, предотвращает развитие атеросклероза.";
			element.ProductForm = "таблетки 100шт.";
			element.ShouldBeCorrected = false;
			element.Description = "";
			element.SideEffect = "Нет сведений";
			element.EnglishName = "";
			element.Storage = "Хранить при комнатной температуре";
			element.Warnings = @"Индивидуальная непереносимость компонентов БАД, детям до 12 лет, беременным и кормящим. 
Перед применением рекомендуется проконсультироваться с врачом";
			DbSession.Save(element);
			DbSession.Flush();
			//_3
			element = new DrugDescription();
			element.Name = "5-НИТРОКС";
			element.UpdateTime = SystemTime.Now();
			element.Composition = @"1 таблетка содержит: нитроксолина - 50 мг.
Вспомогательные вещества: лактозы моногидрат, пшеничный крахмал, тальк, натрий карбоксиметицеллюлоза, стеарат магния, оболочка.";
			element.Dosing =
				"Внутрь, во время или после еды, по 100 мг 4 раза в день, в течение 2-3 нед. В случае необходимости прием можно продолжать по интермиттирующей схеме - по 2 нед в месяц. Максимальная суточная доза - 800 мг. Средняя доза для детей школьного возраста составляет 200-400 мг/сут. Детям младше 5 лет назначают дозу 200 мг/сут, разделенную на 4 приема. Для профилактики инфекций при операциях на почках и мочевыводящих путях - 100 мг 4 раза в сутки в течение 2-3 нед.";
			element.EnglishName = "";
			element.Expiration = "5лет";
			element.IndicationsForUse =
				"Инфекции мочевыводящих путей: пиелонефрит, цистит, уретрит, эпидидимит; профилактика инфекций при различных вмешательствах (катетеризация, цистоскопия, операции на почках и мочеполовых путях).";
			element.Interaction =
				"При одновременном приеме с антацидными средствами, содержащими магний, действие 5-Нитрокса уменьшается. Препарат ухудшает всасывание налидиксовой кислоты";
			element.PharmacologicalAction =
				"Противомикробное средство из группы оксихинолинов. Обладает широким спектром действия. Селективно подавляет синтез бактериальной ДНК, образует комплексы с металлосодержащими ферментами микробной клетки. Оказывает действие на грамположительные бактерии: Staphylococcus spp. (в т.ч. Staphylococcus aureus), Streptococcus spp. (в т.ч. бета-гемолитических стрептококков, Streptococcus pneumoniae, Enterococcus faecalis), Corynebacterium spp., Bacillus subtilis и др. и грамотрицательные бактерии: Escherichia coli, Proteus spp., Klebsiella spp., Salmonella spp., Shigella spp., Enterobacter spp., возбудители гонореи, некоторые др. микроорганизмы - Mycobacterium tuberculosis, Trichomonas vaginalis. Эффективен в отношении некоторых видов грибов (кандиды, дерматофиты, плесень, некоторые возбудители глубоких микозов).";
			element.ProductForm =
				"Таблетки покрытые оболочкой в контурной ячейковой упаковке 10 шт., в картонной пачке 8 упаковок.";
			element.ShouldBeCorrected = false;
			element.Description = "Моча на фоне лечения окрашивается в желто-красный цвет.";
			element.SideEffect =
				"Диспепсия (тошнота, рвота), аллергические реакции (кожная сыпь); тахикардия, атаксия, головная боль, парестезии, полиневропатия, нарушения функции печени.";
			element.EnglishName = "5-Nitrox";
			element.Storage = "В сухом, защищенном от света месте, при температуре 15–25 °C.";
			element.Warnings =
				"Гиперчувствительность, катаракта, неврит, полиневрит, беременность, период лактации, ХПН (олиго-, анурия), печеночная недостаточность, дефицит глюкозо-6-фосфатдегидрогеназы.";
			DbSession.Save(element);
			DbSession.Flush();
		}

		//MNN
		public MNN Clone_MNN(MNN element = null)
		{
			if (element == null) {
				DbSession.Flush();
				element = DbSession.Query<MNN>().FirstOrDefault();
				if (element == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (MNN).Name));
				}
			}
			var newElement = new MNN();
			newElement.RussianValue = element.RussianValue;
			newElement.UpdateTime = element.UpdateTime;
			newElement.Value = element.Value;
			return element;
		}

		public void Generate_MNN()
		{
			//_1
			var element = new MNN();
			element.RussianValue = "Нитроксолин";
			element.UpdateTime = SystemTime.Now();
			element.Value = "Нитроксолин";
			DbSession.Save(element);
			DbSession.Flush();
			//_2
			element = new MNN();
			element.RussianValue = "Fusarium sambuсinum грибы";
			element.UpdateTime = SystemTime.Now();
			element.Value = "Fusarium sambuсinum грибы";
			DbSession.Save(element);
			DbSession.Flush();
			//_3
			element = new MNN();
			element.RussianValue = "Альбумин человека сывороточный йодированный [131I]";
			element.UpdateTime = SystemTime.Now();
			element.Value = "Альбумин человека сывороточный йодированный [131I]";
			DbSession.Save(element);
			DbSession.Flush();
			//_4
			element = new MNN();
			element.RussianValue = "Аммиак+Глицерол+Этанол";
			element.UpdateTime = SystemTime.Now();
			element.Value = "Аммиак+Глицерол+Этанол";
			DbSession.Save(element);
			DbSession.Flush();
			//_5
			element = new MNN();
			element.RussianValue = "Белладонны листьев экстракт+Бензокаин+Метамизол натрия+Натрия гидрокарбонат";
			element.UpdateTime = SystemTime.Now();
			element.Value = "Белладонны листьев экстракт+Бензокаин+Метамизол натрия+Натрия гидрокарбонат";
			DbSession.Save(element);
			DbSession.Flush();
		}

		//DrugFamily
		public DrugForm Clone_DrugForm(DrugForm element = null)
		{
			if (element == null) {
				DbSession.Flush();
				element = DbSession.Query<DrugForm>().FirstOrDefault();
				if (element == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (DrugForm).Name));
				}
			}
			var newElement = new DrugForm();
			newElement.Form = element.Form;
			return element;
		}

		public void Generate_DrugForm()
		{
			//_1
			var element = new DrugForm();
			element.Form = "табл. N100";
			DbSession.Save(element);
			DbSession.Flush();
			//_2
			element = new DrugForm();
			element.Form = "табл. N90";
			DbSession.Save(element);
			DbSession.Flush();
			//_3
			element = new DrugForm();
			element.Form = "табл. 500мг N150";
			DbSession.Save(element);
			DbSession.Flush();
			//_4
			element = new DrugForm();
			element.Form = "табл. 500мг N100";
			DbSession.Save(element);
			DbSession.Flush();
			//_5
			element = new DrugForm();
			element.Form = "спрей наз.(флак.) 0.05% - 14.2 мл N1";
			DbSession.Save(element);
			DbSession.Flush();
			//_6
			element = new DrugForm();
			element.Form = "табл. 50 мг N80";
			DbSession.Save(element);
			DbSession.Flush();
		}

		//DrugFamily
		public DrugFamily Clone_DrugFamily(DrugFamily element = null)
		{
			if (element == null) {
				DbSession.Flush();
				element = DbSession.Query<DrugFamily>().FirstOrDefault();
				if (element == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (DrugFamily).Name));
				}
			}
			var newElement = new DrugFamily();
			newElement.Name = element.Name;
			newElement.DrugDescription = element.DrugDescription;
			newElement.UpdateTime = element.UpdateTime;
			newElement.MNN = element.MNN;
			return element;
		}

		public void Generate_DrugFamily()
		{
			//_1
			var element = new DrugFamily();
			element.Name = "100 СИЛ (БАД)";
			element.UpdateTime = SystemTime.Now();
			element.DrugDescription = DbSession.Query<DrugDescription>().FirstOrDefault(s => s.Name == "100 СИЛ (БАД)");
			element.MNN = DbSession.Query<MNN>().FirstOrDefault();
			DbSession.Save(element);
			DbSession.Flush();
			//_2
			element = new DrugFamily();
			element.Name = "120-80 (БАД)";
			element.UpdateTime = SystemTime.Now();
			element.DrugDescription = DbSession.Query<DrugDescription>().FirstOrDefault(s => s.Name == "120-80");
			element.MNN = DbSession.Query<MNN>().FirstOrDefault();
			DbSession.Save(element);
			DbSession.Flush();
			//_3
			element = new DrugFamily();
			element.Name = "4-ВЭЙ";
			element.UpdateTime = SystemTime.Now();
			element.DrugDescription = DbSession.Query<DrugDescription>().FirstOrDefault();
			element.MNN = DbSession.Query<MNN>().FirstOrDefault();
			DbSession.Save(element);
			DbSession.Flush();
			//_4
			element = new DrugFamily();
			element.Name = "5-НИТРОКС";
			element.UpdateTime = SystemTime.Now();
			element.DrugDescription = DbSession.Query<DrugDescription>().FirstOrDefault();
			element.MNN = DbSession.Query<MNN>().FirstOrDefault(s => s.Value == "Нитроксолин");
			DbSession.Save(element);
			DbSession.Flush();
		}

		//Drug
		public Drug Clone_Drug(Drug element = null)
		{
			if (element == null) {
				DbSession.Flush();
				element = DbSession.Query<Drug>().FirstOrDefault();
				if (element == null) {
					throw new Exception(string.Format("Не возможно клонировать элемент, таблица {0} пуста",
						typeof (Drug).Name));
				}
			}
			var newElement = new Drug();
			newElement.Name = element.Name;
			newElement.DrugFamily = element.DrugFamily;
			newElement.Producers = element.Producers;
			return newElement;
		}

		public void Generate_Drug()
		{
			//_1
			var element = new Drug();
			element.Name = "100 СИЛ (БАД) табл. N100";
			element.DrugFamily = DbSession.Query<DrugFamily>().FirstOrDefault(s => s.Name == "100 СИЛ (БАД)");
			element.DrugForm = DbSession.Query<DrugForm>().FirstOrDefault(s => s.Form == "табл. N100");
			DbSession.Save(element);
			DbSession.Flush();
			//_2
			element = new Drug();
			element.Name = "100 СИЛ (БАД) табл. N90";
			element.DrugFamily = DbSession.Query<DrugFamily>().FirstOrDefault(s => s.Name == "100 СИЛ (БАД)");
			element.DrugForm = DbSession.Query<DrugForm>().FirstOrDefault(s => s.Form == "табл. N90");
			DbSession.Save(element);
			DbSession.Flush();
			//_3
			element = new Drug();
			element.Name = "120-80 (БАД) табл. 500мг N150";
			element.DrugFamily = DbSession.Query<DrugFamily>().FirstOrDefault(s => s.Name == "120-80 (БАД)");
			element.DrugForm = DbSession.Query<DrugForm>().FirstOrDefault(s => s.Form == "табл. 500мг N150");
			DbSession.Save(element);
			DbSession.Flush();
			//_4
			element = new Drug();
			element.Name = "120-80 (БАД) табл. 500мг N100";
			element.DrugFamily = DbSession.Query<DrugFamily>().FirstOrDefault(s => s.Name == "120-80 (БАД)");
			element.DrugForm = DbSession.Query<DrugForm>().FirstOrDefault(s => s.Form == "табл. 500мг N100");
			DbSession.Save(element);
			DbSession.Flush();
			//_5
			element = new Drug();
			element.Name = "4-ВЭЙ спрей наз.(флак.) 0.05% - 14.2 мл N1";
			element.DrugFamily = DbSession.Query<DrugFamily>().FirstOrDefault(s => s.Name == "4-ВЭЙ");
			element.DrugForm = DbSession.Query<DrugForm>().FirstOrDefault(s => s.Form == "спрей наз.(флак.) 0.05% - 14.2 мл N1");
			DbSession.Save(element);
			DbSession.Flush();
			//_6
			element = new Drug();
			element.Name = "5-НИТРОКС табл. 50 мг N80";
			element.DrugFamily = DbSession.Query<DrugFamily>().FirstOrDefault(s => s.Name == "5-НИТРОКС");
			element.DrugForm = DbSession.Query<DrugForm>().FirstOrDefault(s => s.Form == "табл. 50 мг N80");
			DbSession.Save(element);
			DbSession.Flush();
		}
	}
}