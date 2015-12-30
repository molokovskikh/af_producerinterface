using System;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
{
	[Serializable]
	public abstract class IntervalReport : Report, IInterval
	{

		[ScaffoldColumn(false)]
		public DateTime DateFrom { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateTo { get; set; }

	}
}