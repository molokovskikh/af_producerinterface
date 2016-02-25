using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.Linq;


namespace ProducerInterfaceCommon.CustomHelpers
{
    public class Promotions : RegionMaskConverter
    {
     
        public List<OptionElement> GetDrugNamesOnePromotion(int PromotionId)
        {
            return new List<OptionElement>();
        }

        public List<OptionElement> GetGrugNamesPromotions(List<int> PromotionsId)
        {
            return new List<OptionElement>();
        }

        public List<promotions> GetRegionPromotions(decimal RegionMask, int TakeCount, int SkipCount)
        {
            if (RegionMask == 0)
            {
                return GetAllPromotions(TakeCount, SkipCount);
            }

            var MasksList = _cntx.PromotionsInRegionMask((long)RegionMask).ToList();
            List<long> X = MasksList.ToList().Select(x => x.Id).ToList();
            
            var ret = _cntx.promotions.Where(x => X.Contains(x.Id)).ToList();
            
            return new List<promotions>();
        }

        public class PromotionIdOrMask
        {
           public long Id { get; set; }
           public decimal RegionMask { get; set; }
        }


        public List<promotions> GetAllPromotions(int TakeCount, int SkipCount)
        {
            return _cntx.promotions.OrderByDescending(x => x.Id).Skip(SkipCount).Take(TakeCount).ToList();
        }
        

    }
}
