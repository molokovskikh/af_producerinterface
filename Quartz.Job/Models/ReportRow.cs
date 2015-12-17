using System.Collections.Generic;

namespace Quartz.Job.Models
{
	public abstract class ReportRow
	{
		public abstract List<T> Treatment<T>(List<T> list) where T : ReportRow;
	}
}
