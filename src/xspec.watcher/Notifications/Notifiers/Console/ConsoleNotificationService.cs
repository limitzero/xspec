using System;
using System.Collections.Generic;
using xspec.watcher.Notifications.Messages;

namespace xspec.watcher.Notifications.Notifiers.Console
{
	public class ConsoleNotificationService : INotificationService
	{
		private readonly IEnumerable<string> output;

		public void Notify(NotificationMessage message)
		{
			this.Notify(message as dynamic);
		}

		private void Notify(InformationNotificationMessage message)
		{
			this.SendNotification(message.Icon, message);
		}

		private void Notify(SuccessNotificationMessage message)
		{
			this.SendNotification(message.Icon, message);
		}

		private void Notify(FailureNotificationMessage message)
		{
			this.SendNotification(message.Icon, message);
		}

		private void SendNotification(string icon, NotificationMessage message)
		{
			string executable = string.Empty;
			Exception extractExecutableException = null;

			SendNotificationToConsole(message as dynamic);
		}

		private static void SendNotificationToConsole(InformationNotificationMessage message)
		{
			var currentColor = System.Console.ForegroundColor;
			System.Console.ForegroundColor = ConsoleColor.DarkBlue;
			WriteMessage(message.Title, message.Content);
			System.Console.ForegroundColor = currentColor;
		}

		private static void SendNotificationToConsole(SuccessNotificationMessage message)
		{
			var currentColor = System.Console.ForegroundColor;
			System.Console.ForegroundColor = ConsoleColor.DarkGreen;
			WriteMessage(message.Title, message.Content);
			System.Console.ForegroundColor = currentColor;
		}

		private static void SendNotificationToConsole(FailureNotificationMessage message)
		{
			var currentColor = System.Console.ForegroundColor;
			System.Console.ForegroundColor = ConsoleColor.DarkRed;
			WriteMessage(message.Title, message.Content);
			System.Console.ForegroundColor = currentColor;
		}

		private static void WriteMessage(string title, string content)
		{
			System.Console.WriteLine(string.Format("{0} - {1}", title, content));
		}
	}
}