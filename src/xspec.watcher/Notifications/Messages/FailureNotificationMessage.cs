﻿namespace xspec.watcher.Notifications.Messages
{
	public class FailureNotificationMessage : NotificationMessage
	{
		public string Icon { get; private set; }

		public FailureNotificationMessage(string title, string message)
			: base(title, message)
		{
			this.Icon = "green.png";
		}
	}
}