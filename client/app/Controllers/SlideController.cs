using System.Web.Mvc;
using System.Web.UI;

namespace ProducerInterface.Controllers
{
	public class SlideController : ProducerInterfaceCommon.Controllers.BaseController
	{
		// GET: Slide
		[OutputCache(Duration = 600, Location = OutputCacheLocation.Server, VaryByParam = "id")]
		public FileResult GetFile(int id)
		{
			var file = DB2.MediaFiles.Find(id);
			if (file == null) {
				return null;
			}
			return File(file.ImageFile, file.ImageType);
		}
	}
}