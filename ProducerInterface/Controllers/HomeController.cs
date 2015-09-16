﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	public class HomeController : BaseProducerInterfaceController
	{
		//
		// GET: /Главная/
		public ActionResult Index()
		{
			ViewBag.CurrentUser = GetCurrentUser();
			var producers = DbSession.Query<Producer>().ToList();
			ViewBag.Producers = producers;
			return View();
		}
	}
}