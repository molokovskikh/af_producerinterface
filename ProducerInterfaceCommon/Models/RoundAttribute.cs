using System;

namespace ProducerInterfaceCommon.Models
{
	public class RoundAttribute : Attribute
	{
		public int Precision { get; set; }
	}
}