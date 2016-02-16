using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class MediaFilesController : Controller
    {
        // GET: MediaFiles
        public FileResult Index()
        {
            byte[] XXX = new byte[0];
            return File(XXX, "image/png");
        }


        public JsonResult GetFolder()
        {
            return Json("");
        }

        public JsonResult GetFiles(string FolderName)
        {
            return Json("");
        }
    }
}