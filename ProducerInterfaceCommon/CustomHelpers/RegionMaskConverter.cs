using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.CustomHelpers
{
    public class RegionMaskConverter
    {
        protected producerinterface_Entities _cntx = new producerinterface_Entities();

        private List<ulong> ConverterRegionMaskToRegionListUlong(decimal RegionMask)
        {
            return new List<ulong>();
        }

        private decimal ConverterRegionListToRegionMask(List<ulong> ListRegions)
        {
            decimal i = new decimal();
            return i;
        }

        public List<OptionElement> GetRegionList()
        {
            var results = _cntx.regionnames
                .OrderBy(x => x.RegionName)
                .Select(x => new OptionElement { Value = x.RegionCode.ToString(), Text = x.RegionName })
                .ToList();
            return results;
        }

        public List<decimal> GetRegionsMask(ulong RegionMask)
        {
            var ListRegions = _cntx.regionnames.OrderBy(x => x.RegionName).ToList();
            var results = ListRegions.Where(x => ((ulong)x.RegionCode & RegionMask) > 0).OrderBy(x => x.RegionCode).Select(x => x.RegionCode).ToList();
            return results;
        }

        public List<decimal> GetManyRegionsMask(ulong RegionMask)
        {
            int mask = 0;
            List<decimal> ListMaskReturn = new List<decimal>();
            for (int i = 0; i< 64; i++)
            {
                ulong Mask = (ulong)i * (ulong)mask;

                if((Mask & RegionMask) > 0)
                {
                    ListMaskReturn.Add(Mask);
                }
            }

            return ListMaskReturn;
        }
    }
}
