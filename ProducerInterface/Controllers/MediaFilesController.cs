using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class MediaFilesController : MasterBaseController
    {      
        public FileResult GetFile(int Id)
        {
            var File_ = cntx_.promotionsimage.Find(Id);
            return File(File_.ImageFile, File_.ImageType);
        }
    }
}