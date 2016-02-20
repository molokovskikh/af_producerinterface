using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class LogForNetController : MasterBaseController
    {
        // GET: LogForNet
        public string Log { get; set; }

        public ActionResult Index(int Id = 0)
        {
            var MaxCountLogs = cntx_.LogForNet.Count();
            var PagerCount = Convert.ToInt32(GetWebConfigParameters("ErrorCountPage"));

            int Max_Vozmozhniy_ID = (int)Math.Ceiling((decimal)MaxCountLogs / PagerCount);
                  
            if (Max_Vozmozhniy_ID < Id && Id != 0)
            {
                RedirectToAction("Index");
            }

            SortingPagingInfo Info = new SortingPagingInfo();
            Info.CurrentPageIndex = Id;
            Info.PageCount =  Max_Vozmozhniy_ID;          
            ViewBag.Info = Info;
           
            var ModelView = new List<ProducerInterfaceCommon.ContextModels.LogForNet>();


            if (Max_Vozmozhniy_ID > Id && Id != 0)
            {
                // отобразим две кнопки, вперёд и назад
                ModelView = cntx_.LogForNet.OrderByDescending(xxx => xxx.Id).Skip((Id * PagerCount)).Take(PagerCount).ToList();

                ViewBag.Prev = Id + 1;
                ViewBag.Next = Id - 1;
            }
            else if (Max_Vozmozhniy_ID == Id && Id != 0)
            {
                // отобразим только кнопку назад
                ModelView = cntx_.LogForNet.OrderByDescending(xxx => xxx.Id).Skip((Id * PagerCount)).Take(PagerCount).ToList();
                ViewBag.Next = Id - 1;
            }
            else if (Max_Vozmozhniy_ID < Id || Id ==0 )
            {
                if (Id == 0)
                {
                    //
                    ModelView = cntx_.LogForNet.OrderByDescending(xxx => xxx.Id).Skip((0)).Take(PagerCount).ToList();
                    ViewBag.Prev = 1;
                }
                else
                {
                    ModelView = cntx_.LogForNet.OrderByDescending(xxx => xxx.Id).Skip((0)).Take(PagerCount).ToList();
                    if (Max_Vozmozhniy_ID > 1)
                    {
                        ViewBag.Prev = 1;
                    }
                    else
                    {
                       
                    }                  
                }
            }          

            return View(ModelView);
        }
    }
}