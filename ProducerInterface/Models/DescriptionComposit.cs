using System.Collections.Generic;

namespace ProducerInterface.Models
{
	public class DescriptionComposit
	{
		public drugdescription Description { get; set; }

		public drugmnn Mnn { get; set; }

		public IEnumerable<drugformproducer> Forms { get; set; }
	}
}