using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataJsGrid.Controllers
{
    public class DataGridController : Controller
    {
        // GET: DataGrid
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetList(bool sortid = true, string term = null)
        {
            if (term == null)
            {
                if (sortid)
                {
                    return Json(GetListEQP().OrderBy(x => x.equipment_id), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(GetListEQP().OrderByDescending(x => x.equipment_id), JsonRequestBehavior.AllowGet);
                }
            }
            if (sortid)
            {
                return Json(GetListEQP().Where(e => e.equipment_name.Contains(term)).OrderBy(x => x.equipment_id), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetListEQP().Where(e => e.equipment_name.Contains(term)).OrderByDescending(x => x.equipment_id), JsonRequestBehavior.AllowGet);
            }

        }
        public class EQP
        {
            public int equipment_id { get; set; }
            public string equipment_name { get; set; }
        }

        public List<EQP> GetListEQP()
        {
            List<EQP> ret = new List<EQP>();

            for (int I = 1; I < 50; I++)
            {
                ret.Add(new EQP { equipment_id = I, equipment_name = RandomizeName(I) });
            }

            return ret;

        }

        public string RandomizeName(int I)
        {
            string ret = "";

            for (var II = 1; II < I; II++)
            {
                ret += II;
            }

            if (ret == "")
            {
                ret = "Хреновое значение с " + I + "знаков";
            }
            return ret;
        }
    }
}