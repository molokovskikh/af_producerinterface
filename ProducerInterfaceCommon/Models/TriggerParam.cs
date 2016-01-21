using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public abstract class TriggerParam
	{
		[Required]
		[ScaffoldColumn(false)]
		public long UserId { get; set; }

		public abstract List<ErrorMessage> Validate();
	}
}