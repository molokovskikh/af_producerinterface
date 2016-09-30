using System;
using System.IO;
using System.Linq;
using System.Web;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;

namespace ProducerInterfaceCommon.Helpers
{
	public class FileManager
	{
		public static int SaveFile(Context dbContext, HttpPostedFileBase file, EntityType type)
		{
			var model = new MediaFile(file.FileName) {
				ImageType = file.ContentType,
				EntityType = type
			};
			using (var ms = new MemoryStream()) {
				file.InputStream.CopyTo(ms);
				model.ImageFile = ms.ToArray();
				model.ImageSize = ms.Length;
			}
			dbContext.MediaFiles.Add(model);
			dbContext.SaveChanges();
			return model.Id;
		}

		public static bool DeleteFile(Context dbContext, int id)
		{
			var item = dbContext.MediaFiles.FirstOrDefault(s => s.Id == id);
			if (item != null) {
				dbContext.MediaFiles.Remove(item);
				dbContext.SaveChanges();
				return true;
			}
			return false;
		}
	}
}