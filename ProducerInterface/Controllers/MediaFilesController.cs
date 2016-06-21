using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class MediaFilesController : MasterBaseController
	{
		public FileResult GetFile(int Id)
		{
			var File_ = cntx_.MediaFiles.Find(Id);
			return File(File_.ImageFile, File_.ImageType);
		}
	}
}