using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.CustomHelpers.Models
{
    public class PromotionsInRegionMask : BaseModel
    {

        public PromotionsInRegionMask(ulong RegionMask = 0)
        {
            this._regionMask = RegionMask;
        }

        public PromotionsInRegionMask()
        {
        }

        private ulong _regionMask;
        public long Id { get; set; }
        public UInt64 RegionMask { get; set; }
        
        public override string GetSpName()
        {
            return "PromotionsInRegionMask";
        }

        public override Dictionary<string, object> GetSpParams()
        {
            var spparams = new Dictionary<string, object>();
            spparams.Add("@RGM", _regionMask);
            return spparams;
        }

        public override Dictionary<string, object> ViewDataValues()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetHeaders()
        {
            throw new NotImplementedException();
        }
    }

}
