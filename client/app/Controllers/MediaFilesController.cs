using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class MediaFilesController : BaseController
	{
		public FileResult GetFile(int id)
		{
			var model = DB2.MediaFiles.Find(id);
			return File(model.ImageFile, model.ImageType, model.ImageName);
		}
	}
}