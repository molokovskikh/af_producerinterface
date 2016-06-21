﻿using System;
using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class NewsController : MasterBaseController
	{
		/// <summary>
		/// Список новостей
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			ViewBag.Title = "Новости";
			ViewBag.Pager = 1;
			var model = cntx_.NotificationToProducers.Where(x => x.Enabled).OrderByDescending(x => x.DatePublication).Take(10).ToList();
			return View(model);
		}

		/// <summary>
		/// Посмотреть результат
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Description"></param>
		/// <returns></returns>
		[ValidateInput(false)]
		public ActionResult Preview(string Name, string Description)
		{
			var model = new NotificationToProducers()
			{
				DatePublication = DateTime.Now,
				Name = Name,
				Description = Description
			};
			return View(model);
		}

		/// <summary>
		/// Подгрузить предыдущие новости
		/// </summary>
		/// <param name="Pager">pageIndex</param>
		/// <returns></returns>
		public ActionResult GetNextList(int Pager)
		{
			ViewBag.Pager = Pager + 1;
			var model = cntx_.NotificationToProducers.Where(x => x.Enabled).OrderByDescending(x => x.DatePublication).Skip(Pager * 10).Take(10).ToList();
			return PartialView(model);
		}

		/// <summary>
		/// Архив новостей
		/// </summary>
		/// <returns></returns>
		public ActionResult Archive()
		{
			ViewBag.Title = "Архив Новостей";
			var model = cntx_.NotificationToProducers.Where(x => !x.Enabled).OrderByDescending(x => x.DatePublication).ToList();
			return View("Index", model);
		}

		/// <summary>
		/// Перенести новость в архив
		/// </summary>
		/// <param name="Id">идентификатор новости</param>
		/// <returns></returns>
		public ActionResult DeleteNews(long Id)
		{
			// отправляем новость в архив
			var news = cntx_.NotificationToProducers.Find(Id);
			news.Enabled = false;
			cntx_.SaveChanges();

			// добавляем историю изменений
			var history = new NewsChange()
			{
				IdNews = Id,
				IdAccount = CurrentUser.Id,
				DateChange = DateTime.Now,
				TypeCnhange = (byte)NewsActions.Archive,
				IP = CurrentUser.IP
			};
			cntx_.NewsChange.Add(history);
			cntx_.SaveChanges();

			EmailSender.ChangeNewsMessage(cntx_, CurrentUser, NewsActions.Archive, Id, $"{news.Name}\r\n\r\n{news.Description}", "");
			SuccessMessage("Новость отправлена в архив");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Безвозвратно удалить
		/// </summary>
		/// <param name="Id">идентификатор новости</param>
		/// <returns></returns>
		public ActionResult PastRetrieve(long Id)
		{
			var news = cntx_.NotificationToProducers.Find(Id);
			var historyList = cntx_.NewsChange.Where(x => x.IdNews == Id).ToList();
			cntx_.NewsChange.RemoveRange(historyList);
			cntx_.SaveChanges();

			cntx_.Entry(news).State = System.Data.Entity.EntityState.Deleted;
			cntx_.SaveChanges(CurrentUser, "Удаление новости");

			EmailSender.ChangeNewsMessage(cntx_, CurrentUser, NewsActions.PastRetrieve, Id, $"{news.Name}\r\n\r\n{news.Description}", "");
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

			var model = new NotificationToProducers();
			if (Id > 0)
				model = cntx_.NotificationToProducers.Find(Id);
			return View(model);
		}

		/// <summary>
		/// Добавление/внесение изменений
		/// </summary>
		/// <param name="after">заполненная модель новости</param>
		/// <returns></returns>
		[ValidateInput(false)]
		[HttpPost]
		public ActionResult Create(NotificationToProducers after)
		{
			if (!ModelState.IsValid)
				return View(after);

			if (after.Id > 0)
			{
				var before = cntx_.NotificationToProducers.Find(after.Id);
				// добавляем в историю изменения

				var history = new NewsChange()
				{
					IdNews = after.Id,
					IdAccount = CurrentUser.Id,
					DateChange = DateTime.Now,
					TypeCnhange = (byte)NewsActions.Edit,
					IP = CurrentUser.IP,
					NewsNewDescription = after.Description,
					NewsNewTema = after.Name,
					NewsOldDescription = before.Description,
					NewsOldTema = before.Name
				};
				cntx_.NewsChange.Add(history);
				cntx_.SaveChanges();

				// изменияем новость
				before.DatePublication = DateTime.Now;
				before.Description = after.Description;
				before.Name = after.Name;
				before.Enabled = true;
				cntx_.SaveChanges();

				EmailSender.ChangeNewsMessage(cntx_, CurrentUser, NewsActions.Edit, after.Id, $"{before.Name}\r\n\r\n{before.Description}", $"{after.Name}\r\n\r\n{after.Description}");
				SuccessMessage("Изменения успешно сохранены");
			}
			else
			{
				// добавляем новость
				after.DatePublication = DateTime.Now;
				after.Enabled = true;
				cntx_.NotificationToProducers.Add(after);
				cntx_.SaveChanges();

				// пишем в историю
				var history = new NewsChange()
				{
					IdNews = after.Id,
					IdAccount = CurrentUser.Id,
					DateChange = DateTime.Now,
					TypeCnhange = (byte)NewsActions.Add,
					IP = CurrentUser.IP,
					NewsNewDescription = after.Description,
					NewsNewTema = after.Name
				};
				cntx_.NewsChange.Add(history);
				cntx_.SaveChanges();
				EmailSender.ChangeNewsMessage(cntx_, CurrentUser, NewsActions.Add, after.Id, "", $"{after.Name}\r\n\r\n{after.Description}");
				SuccessMessage("Новость успешно добавлена");
			}
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Посмотреть изменения
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public ActionResult History(long Id = 0)
		{
			if (Id == 0)
			{
				ErrorMessage("Неверный номер истории изменений");
				return RedirectToAction("Index");
			}
			var model = cntx_.NewsChange.Find(Id);
			if (model == null)
			{
				ErrorMessage("Неверный номер истории изменений");
				return RedirectToAction("Index");
			}
			return View(model);
		}
	}
}
