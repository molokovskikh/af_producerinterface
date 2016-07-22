using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class NewsController : BaseController
	{
		private string root;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			root = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Content("~")}";
		}

		public ActionResult Index()
		{
			ViewBag.Title = "Новости";
			var model = DB2.Newses.Where(x => x.Enabled).OrderByDescending(x => x.DatePublication).ToList();
			return View(model);
		}

		[ValidateInput(false)]
		public ActionResult Preview(string Name, string Description)
		{
			var model = new News {
				DatePublication = DateTime.Now,
				Subject = Name,
				Body = Description
			};
			return View(model);
		}

		public ActionResult Archive()
		{
			ViewBag.Title = "Архив Новостей";
			var model = DB2.Newses.Where(x => !x.Enabled).OrderByDescending(x => x.DatePublication).ToList();
			return View("Index", model);
		}

		public ActionResult DeleteNews(long id)
		{
			// отправляем новость в архив
			var news = DB2.Newses.Find(id);
			news.Enabled = false;
			DB2.SaveChanges();

			// добавляем историю изменений
			var history = new NewsSnapshot(news, DB2.Users.Find(CurrentUser.Id), "Новость отправлена в архив");
			DB2.NewsHistory.Add(history);
			DB2.SaveChanges();

			Mails.NewsChanged(news, "Новость отправлена в архив", root);
			SuccessMessage("Новость отправлена в архив");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Безвозвратно удалить
		/// </summary>
		/// <param name="id">идентификатор новости</param>
		/// <returns></returns>
		public ActionResult Delete(long id)
		{
			var news = DB2.Newses.Find(id);
			DB2.Newses.Remove(news);
			DB2.SaveChanges();

			Mails.NewsChanged(news, "Новость удалена", root);
			SuccessMessage("Новость удалена");
			return RedirectToAction("Archive");
		}

		/// <summary>
		/// Форма создания/редактирования новости
		/// </summary>
		/// <param name="Id">идентификатор новости</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Create(long Id = 0)
		{
			ViewBag.FullUrlStringFile = GetWebConfigParameters("ImageFullUrlString");

#if DEBUG
			{
				ViewBag.FullUrlStringFile = this.Request.Url.Segments[0] + @"mediafiles/";
			}
#endif

			var model = new News();
			if (Id > 0)
				model = DB2.Newses.Find(Id);
			return View(model);
		}

		[ValidateInput(false)]
		[HttpPost]
		public ActionResult Create(News news)
		{
			if (!ModelState.IsValid)
				return View(news);

			if (news.Id > 0)
			{
				var before = DB2.Newses.Find(news.Id);
				// добавляем в историю изменения

				before.DatePublication = DateTime.Now;
				before.Body = news.Body;
				before.Subject = news.Subject;
				before.Enabled = true;
				DB2.SaveChanges();

				var history = new NewsSnapshot(before, DB2.Users.Find(CurrentUser.Id), "Изменена новость");
				DB2.NewsHistory.Add(history);
				DB2.SaveChanges();

				Mails.NewsChanged(news, "Изменена новость", root);
				SuccessMessage("Изменения успешно сохранены");
			}
			else
			{
				// добавляем новость
				news.DatePublication = DateTime.Now;
				news.Enabled = true;
				DB2.Newses.Add(news);
				DB2.SaveChanges();

				// пишем в историю
				var history = new NewsSnapshot(news, DB2.Users.Find(CurrentUser.Id), "Добавлена новость");
				DB2.NewsHistory.Add(history);
				DB2.SaveChanges();
				Mails.NewsChanged(news, "Добавлена новость", root);
				SuccessMessage("Новость успешно добавлена");
			}
			return RedirectToAction("Index");
		}

		public ActionResult History(long id)
		{
			var model = DB2.NewsHistory.Find(id);
			var old = DB2.NewsHistory
				.Where(x => x.Id < model.Id && x.News.Id == model.News.Id)
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();
			ViewBag.Old = old;
			return View(model);
		}
	}
}
