using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.LoggerModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class LogUserChangeController : BaseController
    {

        public ProducerInterfaceCommon.LoggerModels.Logger_Entities cntx__ = new Logger_Entities();
        
        // GET: LogUserChange
        public ActionResult Index(int Id = 0)
        {
            var MaxLogCount = cntx__.logchangeview.Count();
            var pagerCount = Convert.ToInt32(GetWebConfigParameters("LogCountPage"));


            if (Id > (MaxLogCount / pagerCount))
            {
                return RedirectToAction("Index", new { Id = 0 });
            }

            var ModelView = new List<ProducerInterfaceCommon.LoggerModels.logchangeview>();

            if (Id == 0)
            {
                ModelView = cntx__.logchangeview.OrderByDescending(xxx => xxx.ChangeSetId).Take(pagerCount).ToList();
                if ((MaxLogCount / pagerCount) > 1)
                {
                    ViewBag.Prev = 1;
                }
            }
            else
            {
                ModelView = cntx__.logchangeview.OrderByDescending(xxx => xxx.ChangeSetId).Skip(pagerCount * Id).Take(pagerCount).ToList();

                if ((MaxLogCount / pagerCount) > ((pagerCount * (Id-2))))
                {
                    ViewBag.Prev = Id + 1;
                    ViewBag.Next = Id - 1;
                }
                else
                {
                    ViewBag.Next = Id - 1;
                }
            }

            ViewBag.Pager = pagerCount;

            
            return View(ModelView);
        }

        public ActionResult ReadMore(int Id)
        {

            if (Id == 0)
            {
                return RedirectToAction("Index");
            }
            
            var ViewModel = cntx__.propertychangeview.Where(xxx => xxx.ChangeObjectId == Id).ToList();

            return View(ViewModel);
        }

    }
}