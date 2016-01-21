﻿using System.Collections.Generic;

namespace ProducerInterfaceCommon.Models
{
	public abstract class ReportRow
	{
		public abstract List<T> Treatment<T>(List<T> list) where T : ReportRow;
	}
}
