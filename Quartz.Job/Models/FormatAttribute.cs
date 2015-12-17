using System;

namespace Quartz.Job.Models
{
	public class FormatAttribute : Attribute
	{
		public string Value { get; set; }
	}
}