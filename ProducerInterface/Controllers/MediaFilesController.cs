﻿using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class MediaFilesController : BaseController
	{
		public FileResult GetFile(int Id)
		{
			var File_ = DB.MediaFiles.Find(Id);
			return File(File_.ImageFile, File_.ImageType);
		}
	}
}