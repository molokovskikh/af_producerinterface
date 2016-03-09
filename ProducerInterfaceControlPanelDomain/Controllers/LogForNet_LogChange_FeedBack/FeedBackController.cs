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
            FeedBackFunction FeedBackFunc = new FeedBackFunction(currentUser: CurrentUser);
            var ModelView = FeedBackFunc.GetModelView(FBF);

            if (ModelView.FeedBackList == null || ModelView.FeedBackList.Count() == 0)
            {
                return Json("0", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(ModelView, JsonRequestBehavior.AllowGet);
            }
        }
        
        public JsonResult GetTopHundredList()
        {
            FeedBackFunction FeedBackFunc = new FeedBackFunction(currentUser: CurrentUser);
            
            var ModelView = FeedBackFunc.GetModelView();
            return Json(ModelView, JsonRequestBehavior.AllowGet);
        }
             
    
    }
}