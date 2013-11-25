using System;

namespace watcher.core
{
	public class WatcherExecuterResult
	{
		public bool HaltOnError { get; set; }
		public Exception ExecuterException { get; set; }
	}

	public class WatcherExecuterResult<TResult> : WatcherExecuterResult
	{
		public TResult Result { get; set; }
	}
}