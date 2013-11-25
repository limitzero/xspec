namespace xspec.runner.utility.Notification.Messages
{
	public class InformationNotificationMessage : NotificationMessage
	{
		public string Icon { get; private set; }

		public InformationNotificationMessage(string title, string message)
			: base(title, message)
		{
			this.Icon = "info.png";
		}
	}
}