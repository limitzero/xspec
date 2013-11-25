using System;
using System.Collections.Generic;
using System.Linq;

namespace xSpec.watcher.Internal
{
	public abstract class BaseWatchPipeline : IWatchPipelineExecuter
	{
		public HashSet<IWatchPipelineFilter> Filters { get; private set; }
		public string Result { get; set; }

		protected BaseWatchPipeline()
		{
			this.Filters = new HashSet<IWatchPipelineFilter>();
		}

		public abstract void Configure();

		protected void AddFilter<TFilter>() where TFilter : IWatchPipelineFilter, new()
		{
			var filter = new TFilter();
			AddFilters(filter);
		}

		protected void AddFilters(params IWatchPipelineFilter[] filters)
		{
			AddFiltersInternal(filters);
		}

		public void Execute(string current_directory, string current_file)
		{
			string result = string.Empty;
			Exception previous_filter_exception = null; 

			foreach (var filter in Filters)
			{
				if (previous_filter_exception != null)
				{
					filter.FilterException = previous_filter_exception;
				}
				else
				{
					Exception filter_exception = null;
					if (TryExecuteFilter(filter, current_directory, current_file, out result, out filter_exception) == false)
					{
						previous_filter_exception = filter_exception;
					}
				}
			}

			this.Result = result;
		}

		private bool try_get_next_filter(out IWatchPipelineFilter next_filter, int current_index)
		{
			bool success = false;
			next_filter = null;

			if (current_index == this.Filters.Count || current_index > this.Filters.Count)
				return success;

			next_filter = get_filter_by_index_position_from_hash_set(current_index);

			if (next_filter != null)
				success = true;

			return success;
		}

		private IWatchPipelineFilter get_filter_by_index_position_from_hash_set(int current_index)
		{
			IWatchPipelineFilter next_filter;
			next_filter = new List<IWatchPipelineFilter>(this.Filters.ToList())[current_index];
			return next_filter;
		}

		private static bool TryExecuteFilter(IWatchPipelineFilter filter, string current_directory, string current_file,
			out string result, out Exception filter_exception)
		{
			bool success = false;
			result = string.Empty;
			filter_exception = null;

			try
			{
				filter.Result = result;
				filter.Execute(current_directory, current_file);
				result = filter.Result;
				filter_exception = filter.FilterException;
				success = filter.FilterException == null;
			}
			catch (Exception executed_filter_exception)
			{
				filter_exception = executed_filter_exception;
				Console.WriteLine("filter exception: " + filter_exception);
			}

			return success;
		}

		private void AddFiltersInternal(params IWatchPipelineFilter[] filters)
		{
			foreach (var filter in filters)
			{
				if (this.Filters.Contains(filter) == true) continue;
				this.Filters.Add(filter);
			}
		}
	}
}