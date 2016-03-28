using System.Collections.Generic;

namespace ProducerInterfaceCommon.CustomHelpers.Models
{
	public abstract class BaseModel
	{
		public abstract List<string> GetHeaders();

		public abstract string GetSpName();

		public abstract Dictionary<string, object> GetSpParams();

		public abstract Dictionary<string, object> ViewDataValues();
	}
}
