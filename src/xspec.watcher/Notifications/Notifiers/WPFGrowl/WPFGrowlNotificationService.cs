using System;
using System.Diagnostics;
using System.IO;
using xspec.watcher.Notifications.Messages;

namespace xspec.watcher.Notifications.Notifiers.WPFGrowl
{
	public class WPFGrowlNotificationService : INotificationService
	{
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

			if(TryExtractNotifiactionExecutable(out executable, out extractExecutableException) == true)
			{
				SendNotificationToExternalProgram(executable, message as dynamic);
			}
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

		private static void SendNotificationToExternalProgram(string executablePath, InformationNotificationMessage message)
		{
			InvokeExecutable(executablePath, 
				"/level:info", 
				string.Format("/title:\"{0}\"", message.Title), 
				string.Format("/message:\"{0}\"", message.Content) 
				);
		}

		private static void SendNotificationToExternalProgram(string executablePath, SuccessNotificationMessage message)
		{
			InvokeExecutable(executablePath,
				"/level pass",
				string.Format("/title:\"{0}\"", message.Title),
				string.Format("/message:\"{0}\"", message.Content)
				);
		}

		private static void SendNotificationToExternalProgram(string executablePath, FailureNotificationMessage message)
		{
			InvokeExecutable(executablePath,
				"/level:fail",
				string.Format("/title:\"{0}\"", message.Title),
				string.Format("/message:\"{0}\"", message.Content)
				);
		}

		private static void InvokeExecutable(string pathToExecutable, params string[] arguements)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(pathToExecutable);

			if (arguements.Length > 0)
			{
				string args = string.Join(" ", arguements);
				startInfo.Arguments = args;
			}

			// kill the process if it is already running
			Process[] notificationProcesses = System.Diagnostics.Process
				.GetProcessesByName(Path.GetFileNameWithoutExtension(pathToExecutable));

			if(notificationProcesses.Length > 0)
			{
				foreach (var notificationProcess in notificationProcesses)
					notificationProcess.Kill();
			}

			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = false;

			using (var process = new Process())
			{
				process.StartInfo = startInfo;
				process.Start();

				process.WaitForExit(TimeSpan.FromSeconds(10).Milliseconds);
			}

			// kill the process if it is already running
			notificationProcesses = System.Diagnostics.Process
				.GetProcessesByName(Path.GetFileNameWithoutExtension(pathToExecutable));

			if (notificationProcesses.Length > 0)
			{
				foreach (var notificationProcess in notificationProcesses)
					notificationProcess.Kill();
			}
		}

		private static void WriteMessage(string title, string content)
		{
			System.Console.WriteLine(string.Format("{0} - {1}", title, content));
		}

		private static bool TryExtractNotifiactionExecutable(out string executable, out Exception loadNotificationExecutableException)
		{
			bool success = false;
			executable = string.Empty;
			loadNotificationExecutableException = null;

			try
			{
				executable = System.Configuration.ConfigurationManager.AppSettings.Get("notification.executable");
				success = string.IsNullOrEmpty(executable) == false;
			}
			catch (Exception tryGetExecutablePathException)
			{
				loadNotificationExecutableException = tryGetExecutablePathException;
			}

			return success;
		}
	}
}