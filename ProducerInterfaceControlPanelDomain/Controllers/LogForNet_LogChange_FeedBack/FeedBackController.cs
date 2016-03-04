using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack;
using System;
using System.Collections.Generic;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class FeedBackController : MasterBaseController
    {
        public ActionResult Index()
        {       
            return View("FeedBackList");
        }

        public JsonResult FeedbackFilter(FeedBackFilter FBF)
        {
            return Json("0", JsonRequestBehavior.AllowGet);

        }
        
        public JsonResult GetTopHundredList()
        {
            FeedBackFunction FeedBackFunc = new FeedBackFunction(currentUser: CurrentUser);
            
            var ModelView = FeedBackFunc.GetModelView();
            return Json(ModelView, JsonRequestBehavior.AllowGet);
        }
             
    
    }
}