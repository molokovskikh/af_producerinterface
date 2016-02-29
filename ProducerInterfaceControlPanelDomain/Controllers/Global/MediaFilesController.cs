using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class MediaFilesController : MasterBaseController
    {

        public ActionResult Index()
        {
            ViewBag.FullUrlStringFile = GetWebConfigParameters("ImageFullUrlString");

#if DEBUG
{
                ViewBag.FullUrlStringFile = this.Request.Url.Segments[0] + @"mediafiles/";
}
#endif

            var Files = cntx_.promotionsimage.Where(xxx=>xxx.NewsOrPromotions == true).ToList();
            return View(Files);
        }


        public FileResult GetFile(int Id)
        {
            var File_ = cntx_.promotionsimage.Find(Id);
            return File(File_.ImageFile, File_.ImageType);
        }
            
        public ActionResult SaveFile()
        {
            var NewsFile = new ProducerInterfaceCommon.ContextModels.promotionsimage();

            for (int i = 0; i < this.HttpContext.Request.Files.Count; i++)
            {
               
                MemoryStream MS = new MemoryStream();
                HttpPostedFileBase HPFB = this.HttpContext.Request.Files[i];
                HPFB.InputStream.CopyTo(MS);

                NewsFile.ImageFile = MS.ToArray();
                NewsFile.ImageName = HPFB.FileName.Split(new Char[] { '\\' }).Last();
                NewsFile.ImageType = HPFB.ContentType;
                NewsFile.ImageSize = MS.Length.ToString();
                NewsFile.NewsOrPromotions = true; // false - promofile  true - news file
                cntx_.promotionsimage.Add(NewsFile);
                cntx_.SaveChanges();                
            }
            string CKEditorFuncNum = HttpContext.Request["CKEditorFuncNum"];
                  
          //  string url = "/mediafiles/GetFile/" + NewsFile.Id;

            string url = GetWebConfigParameters("ImageFullUrlString");
            url += "/GetFile/" + NewsFile.Id;

            return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + url + "\");</script>");

        }

    }
}