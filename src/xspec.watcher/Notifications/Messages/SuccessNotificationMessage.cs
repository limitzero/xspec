namespace xspec.watcher.Notifications.Messages
{
	public class SuccessNotificationMessage : NotificationMessage
	{
		public string Icon { get; private set; }

		public SuccessNotificationMessage(string title, string message)
			: base(title, message)
		{
			this.Icon = "green.png";
		}
	}
}