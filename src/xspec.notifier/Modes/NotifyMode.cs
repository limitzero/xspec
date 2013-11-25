using System;
using System.Collections.Specialized;
using WPFGrowlNotification.Runner;

namespace WPFGrowlNotification.Modes
{
	public class NotifyMode : IRunMode
	{
		public Notification Execute(ExecutingOptions executingOptions)
		{
			var title = executingOptions.Title;
			var message = executingOptions.Message;

			var level = executingOptions.Level;
			NotificationLevel parsed;
			if (Enum.TryParse<NotificationLevel>(level.Trim(), true, out parsed))
				level = parsed.ToString().ToLower();
			else
				level = NotificationLevel.Info.ToString().ToLower();

			
			return new Notification
					{
						Title = title,
						ImageUrl = string.Format("pack://application:,,,/Resources/{0}.png", level),
						Message = message
					};
		}
	}
}