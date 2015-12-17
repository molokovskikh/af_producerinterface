using System;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
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
