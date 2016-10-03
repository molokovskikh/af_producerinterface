using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class MediaFilesController : BaseController
	{
		public FileResult GetFile(int id)
		{
			var model = DB2.MediaFiles.Find(id);
#if DEBUG
		    if (model == null) {
		        return null;
		    }
#endif
            return File(model.ImageFile, model.ImageType, model.ImageName);
		}
	}
}