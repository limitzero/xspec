using System;
using System.Collections.Generic;

namespace watcher.core
{
	public abstract class BaseWatcherExecutionPipeline
	{
		public HashSet<IWatcherActionFilter> Filters { get; private set; }

		protected BaseWatcherExecutionPipeline()
		{
			this.Filters = new HashSet<IWatcherActionFilter>();
		}

		public abstract void Configure();

		protected void AddFilter<TFilter>() where TFilter : IWatcherActionFilter, new()
		{
			var filter = new TFilter();
			AddFilters(filter);
		}

		protected void AddFilters(params IWatcherActionFilter[] filters)
		{
			AddFiltersInternal(filters);
		}

		public void Execute(FileChangedEvent fileChangedResult)
		{
			WatcherExecuterResult watcherExecuterResult = new WatcherExecuterResult();

			foreach (var filter in Filters)
			{
				if (TryExecuteWatchActionFilter(filter, watcherExecuterResult, fileChangedResult) == false)
				{
					if (watcherExecuterResult.ExecuterException != null
					    && watcherExecuterResult.HaltOnError == true)
					{
						break;
					}
				}
			}
		}

		private bool TryExecuteWatchActionFilter(IWatcherActionFilter filter, 
			WatcherExecuterResult watcherExecuterResult, 
			FileChangedEvent fileChangedResult)
		{
			bool success = false;
			
			try
			{
				watcherExecuterResult =  filter.Execute(watcherExecuterResult, fileChangedResult);
				success = true;
			}
			catch (Exception executed_filter_exception)
			{
				watcherExecuterResult.ExecuterException = executed_filter_exception;
			}

			return success;
		}

		private void AddFiltersInternal(params IWatcherActionFilter[] filters)
		{
			foreach (var filter in filters)
			{
				if (this.Filters.Contains(filter) == true) continue;
				this.Filters.Add(filter);
			}
		}
	}
}