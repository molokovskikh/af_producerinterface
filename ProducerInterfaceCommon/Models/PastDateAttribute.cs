using System;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	class PastDateAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			return value is DateTime &&
						((DateTime)value) < DateTime.Now;
		}
	}
}
