namespace watcher.build.test.connector.Internal
{

	public interface IGrowlMessage
	{
		string Icon { get; }
		string Title { get; set; }
		string Message { get; set; }
	}

	public class SuccessGrowlMessage : IGrowlMessage
	{
		public string Icon { get; private set; }
		public string Title { get; set; }
		public string Message { get; set; }

		public SuccessGrowlMessage()
		{
			Icon = "green.png";
		}
	}

	public class FailureGrowlMessage : IGrowlMessage
	{
		public string Icon { get; private set; }
		public string Title { get; set; }
		public string Message { get; set; }

		public FailureGrowlMessage()
		{
			Icon = "red.png";
		}
	}

	public class GrowlMessageSender
	{
		public void SendMessage(IGrowlMessage message)
		{
			var content = message.Message.Replace("\r\n", "\n");

		}
	}
}