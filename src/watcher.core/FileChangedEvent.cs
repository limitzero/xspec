using System.IO;

namespace watcher.core
{
	public class FileChangedEvent
	{
		public WatcherChangeTypes ChangeType { get; set; }
		public string WatchedDirectory { get; set; }
		public string File { get; set; }
		public string Directory { get; set; }
	}
}
