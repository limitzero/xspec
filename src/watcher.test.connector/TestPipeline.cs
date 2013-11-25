using watcher.core;

namespace watcher.test.connector
{
	public class TestPipeline : BaseWatcherExecutionPipeline
	{
		public override void Configure()
		{
			this.AddFilter<EchoActionFilter>();
		}
	}
}