using System;
using System.Collections.Generic;
using System.Text;
using xspec.watcher.Notifications.Messages;
using xspec.watcher.Notifications.Notifiers.Console;
using xspec.watcher.Notifications.Notifiers.WPFGrowl;

namespace xspec.watcher.BuildRunners.MSBuild
{
	public class MSBuildOutputParser
	{
		private readonly IEnumerable<string> output;

		public MSBuildOutputParser(IEnumerable<string> output)
		{
			this.output = output;
		}

		public void Parse(ref bool success)
		{
			var lines = new List<string>(this.output);
			StringBuilder builder = new StringBuilder();

			foreach (var line in lines)
			{
				if(line.Contains("Microsoft") || string.IsNullOrEmpty(line.Trim())) continue;
				builder.AppendLine(line);
			}

			var notificationService = new ConsoleNotificationService();
			NotificationMessage message = null;
			
			if (builder.Length == 0)
			{
				message = new SuccessNotificationMessage("Build", "Success");
				success = true;
			}
			else
			{
				message = new FailureNotificationMessage("Build Failed", builder.ToString());
				success = false;
			}

			notificationService.Notify(message);
		}

	}
}