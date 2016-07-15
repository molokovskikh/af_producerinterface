using System.Collections.Generic;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceCommon.Helpers
{
	public static class UIHelper
	{
		public static List<OptionElement> ToOptions(this IEnumerable<Region> regions)
		{
			return regions.Select(x => new OptionElement { Value = x.Id.ToString(), Text = x.Name }).ToList();
		}
	}
}