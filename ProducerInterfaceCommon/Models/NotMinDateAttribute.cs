using System;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	class NotMinDateAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			return value is DateTime &&
						((DateTime)value) > DateTime.MinValue;
		}
	}
}
