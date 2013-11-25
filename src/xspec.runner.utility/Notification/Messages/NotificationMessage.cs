namespace xspec.runner.utility.Notification.Messages
{
	public abstract class NotificationMessage
	{
		public string Title { get; private set; }
		public string Content { get; private set; }

		protected NotificationMessage(string title, string message)
		{
			Title = title;
			Content = message;
		}
	}
}