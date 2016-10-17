using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Server.BackgroundServices
{
	interface IBackgroundService
	{
		CancellationToken Cancellation { get; set; }
		int RepeatInterval { get; }
		void Execute();
	}
}
