using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Helpers
{
	public class CollectionRequired : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			return (value as ICollection)?.Count > 0;
		}
	}
}