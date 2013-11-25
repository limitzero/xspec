using System;

namespace xSpec.watcher.Internal
{
	public interface IWatchPipelineFilter : IWatchPipelineExecuter
	{
		string Result { get; set; }
		Exception FilterException { get; set; }
	}
}