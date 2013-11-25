using System;
using System.Collections.Generic;
using System.Text;
using xspec.watcher.Notifications;
using xspec.watcher.Notifications.Messages;

namespace xspec.watcher.TestRunners.XSpecConsole
{
	public class XSpecConsoleTestRunnerOutputParser
	{
		private readonly INotificationService notification_service;
		private readonly IEnumerable<string> lines;

		public XSpecConsoleTestRunnerOutputParser(INotificationService notificationService, IEnumerable<string> lines)
		{
			notification_service = notificationService;
			this.lines = lines;
		}

		public void Parse()
		{
			NotificationMessage message = null;
			StringBuilder builder = new StringBuilder();

			string results = string.Join(System.Environment.NewLine, lines);
			Console.WriteLine(results); 

			foreach (var line in lines)
			{
				if (line.Trim().StartsWith("it") && line.Trim().Contains("FAILED"))
					builder.AppendFormat(">> {0}", line.Trim());

				if (line.Trim().Contains("Exception"))
					builder.AppendFormat(">> {0}", line.Trim());

				if (line.Trim().StartsWith("*") || line.Trim().StartsWith("="))
					break;
			}

			if (builder.Length > 0)
				message = new FailureNotificationMessage("Test Failure", builder.ToString());
			else
			{
				message = new SuccessNotificationMessage("Test","Success");
			}

			if (this.notification_service != null)
			{
				this.notification_service.Notify(message);
			}
		}
	}
}