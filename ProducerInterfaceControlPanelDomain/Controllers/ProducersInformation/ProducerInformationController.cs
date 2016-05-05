using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public partial class ProducerInformationController : MasterBaseController
	{

		[HttpGet]
		public ActionResult Index()
		{
			// ListProducers - список не только компаниий производителей но и компаний с анонимным производителем 
			// точнее с анонимными пользователями.

			var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
			ViewBag.ListProducer = h.RegisterListProducer();
			var ListProducers = cntx_.AccountCompany.ToList();
			return View(ListProducers);
		}

		public ActionResult GetProducerInformation(int Id) // Id - table AccountCompany
		{

			var firstUserId = cntx_.Account.Where(x => x.AccountCompany.Id == Id).First().Id;
			var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, firstUserId);
			ViewBag.ListProducer = h.RegisterListProducer();
			ViewBag.PromotionList = cntx_.promotions.Where(xxx => xxx.ProducerId == cntx_.AccountCompany.Where(x => x.Id == Id).FirstOrDefault().ProducerId).ToList();

			var PromotionList = cntx_.promotions.Where(xxx => xxx.ProducerId == cntx_.AccountCompany.Where(x => x.Id == Id).FirstOrDefault().ProducerId).ToList();

			foreach (var ItemPromotion in PromotionList)
			{
				ItemPromotion.GetRegionnamesList();
			}

			ViewBag.DrugList = h.GetCatalogListPromotion();

			ViewBag.ReportList = cntx_.jobextendwithproducer.Where(x => x.ProducerId == cntx_.AccountCompany.Where(vvv => vvv.Id == Id).FirstOrDefault().ProducerId).ToList();


			var AccountCompanyView = cntx_.AccountCompany.Find(Id);
			return PartialView("partial/producerinformation", AccountCompanyView);
		}

		public ActionResult RequestAccount()
		{
			var model = cntx_.Account.Where(xxx => xxx.AccountCompany.ProducerId == null).ToList();
			return View(model);
		}

	}
}