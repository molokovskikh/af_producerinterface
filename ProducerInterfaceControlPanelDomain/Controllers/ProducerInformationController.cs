using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System.ComponentModel.DataAnnotations;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class ProducerInformationController : BaseController
	{

		/// <summary>
		/// Список зарегистрированных компаний (!)
		/// </summary>
		/// <param name="companyId">идентификатор компании</param>
		/// <returns></returns>
		public ActionResult DomainList(long? companyId)
		{
			SetViewBag(companyId);
			if (companyId.HasValue)
			{
				var model = DB.CompanyDomainName.Where(x => x.CompanyId == companyId.Value).ToList();
				return View(model);
			}
			return View();
		}

		/// <summary>
		/// Добавление нового домена к компании
		/// </summary>
		/// <param name="companyId">идентификатор компании</param>
		/// <param name="domainName">имя нового домена</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddDomain(long companyId, string domainName)
		{
			if (string.IsNullOrEmpty(domainName))
			{
				SetViewBag(companyId);
				ViewBag.Error = "Укажите имя домена";
				ViewBag.DomainName = domainName;
				var model = DB.CompanyDomainName.Where(x => x.CompanyId == companyId).ToList();
				return View("DomainList", model);
			}

			var ea = new EmailAddressAttribute();
			if (!ea.IsValid($"ab@{domainName}"))
			{
				SetViewBag(companyId);
				ViewBag.Error = "Неверный формат домена";
				ViewBag.DomainName = domainName;
				var model = DB.CompanyDomainName.Where(x => x.CompanyId == companyId).ToList();
				return View("DomainList", model);
			}

			var domainExist = DB.CompanyDomainName.SingleOrDefault(x => x.CompanyId == companyId && x.Name == domainName);
			if (domainExist != null)
			{
				SetViewBag(companyId);
				ViewBag.Error = $"Домен {domainName} уже есть в списке данного производителя";
				ViewBag.DomainName = domainName;
				var model = DB.CompanyDomainName.Where(x => x.CompanyId == companyId).ToList();
				return View("DomainList", model);
			}

			var newDomain = new CompanyDomainName() { CompanyId = companyId, Name = domainName };
			DB.CompanyDomainName.Add(newDomain);
			DB.SaveChanges();
			return RedirectToAction("DomainList", new { companyId = companyId });
		}

		private void SetViewBag(long? companyId)
		{
			ViewBag.CompanyId = companyId;
			ViewBag.CompanyList = DB.AccountCompany
				.OrderBy(x => x.Name)
				.Select(x => new SelectListItem() { Text = x.ProducerId.HasValue ? x.Name : x.Name + " (без производителя)", Value = x.Id.ToString(), Selected = x.Id == companyId })
				.ToList();
		}

		/// <summary>
		/// Удаление домена
		/// </summary>
		/// <param name="Id">идентификатор домена</param>
		/// <returns></returns>
		public ActionResult DeleteDomain(long Id)
		{
			var domain = DB.CompanyDomainName.Find(Id);
			var companyId = domain.CompanyId;
			var domainList = DB.CompanyDomainName.Where(x => x.CompanyId == companyId).ToList();

			// последний домен нельзя удалить
			if (domainList.Count() > 1)
			{
				DB.CompanyDomainName.Remove(domain);
				DB.SaveChanges();
				SuccessMessage($"Домен {domain.Name} удален");
			}
			// последний домен нельзя удалить
			else
				ErrorMessage($"Домен {domain.Name} не удален, нельзя удалить последнее доменное имя производителя");

			return RedirectToAction("DomainList", new { companyId = companyId });
		}

		/// <summary>
		/// Статистика по производителям (выбор производителя)
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Index()
		{
			var h = new NamesHelper(CurrentUser.Id);
			ViewBag.ListProducer = h.RegisterListProducer();
			var model = DB.AccountCompany.Where(x => x.ProducerId.HasValue).ToList(); // TODO протестировать без производителя
			return View(model);
		}

		/// <summary>
		/// Вывод статистики выбранного производителя
		/// </summary>
		/// <param name="Id">идентификатор AccountCompany</param>
		/// <returns></returns>
		public ActionResult GetProducerInformation(int Id)
		{
			var producerId = DB.AccountCompany.Single(x => x.Id == Id).ProducerId;
			var promotionList = DB2.Promotions.Where(x => x.ProducerId == producerId).ToList();

			var firstUserId = DB.Account.First(x => x.AccountCompany.Id == Id).Id;
			var h = new NamesHelper(firstUserId);
			ViewBag.ListProducer = h.RegisterListProducer();
			ViewBag.PromotionList = promotionList;

			foreach (var item in promotionList)
				item.GetRegionnamesList();

			ViewBag.DrugList = h.GetCatalogListPromotion();
			ViewBag.ReportList = DB.jobextendwithproducer.Where(x => x.ProducerId == producerId).ToList();

			var model = DB.AccountCompany.Find(Id);
			return PartialView("partial/producerinformation", model);
		}
	}
}