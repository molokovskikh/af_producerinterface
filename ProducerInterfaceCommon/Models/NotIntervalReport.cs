using System;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public abstract class NotIntervalReport : Report, INotInterval
	{
		[ScaffoldColumn(false)]
		public DateTime DateFrom { get; set; }
	}
}