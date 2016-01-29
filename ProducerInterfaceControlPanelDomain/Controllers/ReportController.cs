using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class ReportController : MasterBaseController
    {        // GET: Report
       
        private string shedName_ = ""; // DebugShedulerName(); 

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            // ViewBag.currentUser = User.Identity.Name;
            shedName_ = DebugShedulerName();
        }
        
        [HttpGet]
        public ActionResult Index()
        {           
            ViewBag.ProducerList = GetProducerList();

            var Model = new SearchProducerReportsModel();

            return View(Model);
        }

    

        [HttpPost]
        public ActionResult SearchReports(SearchProducerReportsModel Models_id_Producer)
        {
            ViewBag.Searh = "Search";          

            if (ModelState.IsValid)
            {
                List<jobextendwithproducer> ListReport = cntx_.jobextendwithproducer.Where(xxx => xxx.ProducerId == Models_id_Producer.Producers).ToList();                         
                ViewBag.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == Models_id_Producer.Producers).ToList().First().ProducerName;
                return View("SearchResult", ListReport);
            }
            else
            {
                ViewBag.ProducerList = GetProducerList();
                return View("Index", Models_id_Producer);
            }        
        }

        public ActionResult ActiveReport(int Id=0)
        {
            var CounMax = cntx_.jobextend.Where(xxx=>xxx.Enable == true)
                .Where(xxx=>xxx.SchedName == shedName_).Count();
            int CountListInOnePage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
            int Skip_ = Id * CountListInOnePage;   
                   
            ViewBag.CountPage = CountListInOnePage;

            int PagerMax =(CounMax/ CountListInOnePage);

            if (PagerMax == Id && Id != 0)
            {
                ViewBag.Preview = (Id - 1);
            }
            else if (PagerMax < Id && Id != 0)
            {
                ViewBag.Next = 1;
            }
            else if (PagerMax > Id && Id != 0)
            {
                ViewBag.Preview = (Id - 1);
                ViewBag.Next = (Id + 1);
            }
            else if (PagerMax > Id && Id == 0)
            {
                ViewBag.Next = (Id + 1);
            }
            else if (PagerMax < Id && Id == 0)
            {
                // кнопок для листания не будет, если список меньше количества выводимых записей
            }
            
            var ListRep = GetListReportActiveNot(true, Id, CountListInOnePage, DebugShedulerName(), CounMax);
                      
            return View("SearchResult", ListRep);     

        }

        public ActionResult NotActiveReport(int Id = 0)
        {
            var CounMax = cntx_.jobextend.Where(xxx => xxx.Enable == false).Count();
            int CountListInOnePage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
            int Skip_ = Id * CountListInOnePage;

            int PagerMax = (CounMax / CountListInOnePage);

            if (PagerMax == Id)
            {
                ViewBag.Preview = (Id - 1);
            }
            else if (PagerMax < Id && Id != 0)
            {
                ViewBag.Next = 1;
            }
            else if (PagerMax > Id && Id != 0)
            {
                ViewBag.Preview = (Id - 1);
                ViewBag.Next = (Id + 1);
            }
            else if (PagerMax > Id && Id == 0)
            {
                ViewBag.Next = (Id + 1);
            }
            else if (PagerMax < Id && Id == 0)
            {
                // кнопок для листания не будет, если список меньше количества выводимых записей
            }
                
            var ListRep = GetListReportActiveNot(false, Id, CountListInOnePage, DebugShedulerName(), CounMax);
                
            return View("SearchResult", ListRep);

        }                
        
        public ActionResult RunReportsList(Guid GuidReport)
        {
            // Список запусков текущего отчёта
            return View();
        }

        public List<jobextendwithproducer> GetListReportActiveNot(bool activeRep, int Pager, int CountInOnePage, string ShedullerName, int CounMax)
        {
            int Skip_ = Pager * CountInOnePage;

            if (CounMax <= Skip_)
            {
                Skip_ = 0;
            }

            var ListReport = cntx_.jobextendwithproducer
                .Where(xxx => xxx.Enable == activeRep)
                .Where(xxx => xxx.SchedName == ShedullerName)
                .ToList()
               .OrderBy(xxx => xxx.CreationDate).Skip(Skip_).Take(CountInOnePage).ToList();

            return ListReport;
        }

        public List<OptionElement> GetProducerList()
        {
            var X = cntx_.producernames.ToList().Select(xxx => new OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList();
            var Y = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
            Y.AddRange(X);
            return Y;
        }


    }
}