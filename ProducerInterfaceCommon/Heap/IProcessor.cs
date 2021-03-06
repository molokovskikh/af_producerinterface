﻿using ProducerInterfaceCommon.Models;
using Quartz;
using System.Data;
using System.IO;

namespace ProducerInterfaceCommon.Heap
{
	public interface IProcessor
	{
		void Process(JobKey key, Report report, TriggerParam tparam);

		FileInfo CreateExcel(string jobGroup, string jobName, DataSet ds, Report report);
	}
}