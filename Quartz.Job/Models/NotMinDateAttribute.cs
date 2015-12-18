using System;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
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
