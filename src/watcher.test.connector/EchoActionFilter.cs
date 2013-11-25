using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using watcher.core;

namespace watcher.test.connector
{
	public class EchoActionFilter : IWatcherActionFilter
	{
		public WatcherExecuterResult Execute(WatcherExecuterResult watcherExecuterResult,
			FileChangedEvent fileChangedResult)
		{
			// echo changed file to console:
			Console.WriteLine(fileChangedResult.File);
			return watcherExecuterResult;
		}
	}
}
