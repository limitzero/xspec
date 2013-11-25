namespace watcher.core
{
	public interface IWatcherActionFilter
	{
		WatcherExecuterResult Execute(WatcherExecuterResult watcherExecuterResult, FileChangedEvent fileChangedResult);
	}
}