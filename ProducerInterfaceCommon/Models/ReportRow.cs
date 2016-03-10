using System.Collections.Generic;

namespace ProducerInterfaceCommon.Models
{
	public abstract class ReportRow
	{
		public abstract IEnumerable<T> Treatment<T>(IEnumerable<T> list, Report param) where T : ReportRow;
	}
}
