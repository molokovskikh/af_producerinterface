using System;

namespace Quartz.Job.Models
{
	public class RoundAttribute : Attribute
	{
		public int Precision { get; set; }
	}
}