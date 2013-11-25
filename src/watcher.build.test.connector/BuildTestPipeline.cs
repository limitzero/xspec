using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using watcher.build.test.connector.Filters;
using watcher.core;

namespace watcher.build.test.connector
{
	public class BuildTestPipeline : BaseWatcherExecutionPipeline
	{
		public override void Configure()
		{
			this.AddFilter<BuildActionFilter>();
			this.AddFilter<TestActionFilter>();
		}
	}
}
