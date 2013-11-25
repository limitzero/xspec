using System;
using watcher.build.test.connector.Internal;
using watcher.core;

namespace watcher.build.test.connector.Filters
{
	public class TestActionFilter : IWatcherActionFilter
	{
		public WatcherExecuterResult Execute(WatcherExecuterResult watcherExecuterResult, 
		                                     FileChangedEvent fileChangedResult)
		{
			string result = new CommandFileExecutor().Execute("test.cmd");
			var execution_result = new WatcherExecuterResult<string>();
			execution_result.Result = result; 

			Console.WriteLine(result);

			return watcherExecuterResult;
		}
	}
}