using System.Linq;
using System.Web.Mvc;
using Analit.Components;
using AnalitFramefork.Components;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	/// <summary>
	///     Страница профиля пользователя
	/// </summary> 
	public class ProfileController : BaseProducerInterfaceController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Профиль пользователя";
		}

		public ActionResult Index()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			var currentUser = DbSession.Query<ProducerUser>().FirstOrDefault(e => e.Name == User.Identity.Name);
			ViewBag.CurrentUser = currentUser;
			ViewBag.ProducerUserList =
				DbSession.Query<ProducerUser>().Where(e => e.Producer == currentUser.Producer).OrderBy(s => s.Name).ToList();
			ViewBag.ProfileNewsList =
				DbSession.Query<ProfileNews>().Where(e => e.Producer == currentUser.Producer).OrderByDescending(s => s.CreatedDate).ThenBy(s=>s.Topic).ToList();
			return View("Index");
		}

		public ActionResult Catalog(int? id = null)
		{
			if (id.HasValue)
			{
				var family  = DbSession.Query<DrugFamily>().FirstOrDefault(i => i.Id == id.Value);
				ViewBag.DrugFamily = family;
			}
			return View("Catalog");
		}

		public ActionResult CreateDrugDescriptionRemark(int id)
		{
			var family = DbSession.Query<DrugFamily>().First(i => i.Id == id);
			var remark = new DrugDescriptionRemark(family);
			var mnns = DbSession.Query<MNN>().ToList();
			ViewBag.AvailibleMnn = mnns;
			ViewBag.DrugDescriptionRemark = remark;
            return View("CreateDrugDescriptionRemark");
		}

		[HttpPost]
		public ActionResult CreateDrugDescriptionRemark(int id, [EntityBinder] DrugDescriptionRemark drugDescriptionRemark)
		{
			var user = GetCurrentUser();
			var family = DbSession.Query<DrugFamily>().First(i => i.Id == id);
			drugDescriptionRemark.ProducerUser = user;
			drugDescriptionRemark.DrugFamily = family;
			var errors = ValidationRunner.Validate(drugDescriptionRemark);
			if (errors.Length == 0)
			{
				DbSession.Save(drugDescriptionRemark);
				SuccessMessage("Запрос на изменение описания отправлен модератору.");
				return Redirect(GetIndexActionUrl());
			}
			ErrorMessage("Произошла ошибка.");
			CreateDrugDescriptionRemark(id);
			ViewBag.DrugDescriptionRemark = drugDescriptionRemark;
            return View("CreateDrugDescriptionRemark");
		}
	}
}