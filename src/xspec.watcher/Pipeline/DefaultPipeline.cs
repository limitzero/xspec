using xSpec.watcher.Internal;

namespace xSpec.watcher.Pipeline
{
	public class DefaultPipeline : BaseWatchPipeline
	{
		public override void Configure()
		{
			this.AddFilter<BuildFilter>();
			this.AddFilter<TestFilter>();
		}
	}
}