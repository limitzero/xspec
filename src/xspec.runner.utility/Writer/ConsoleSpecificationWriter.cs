using System;
using System.Diagnostics;
using System.Text;
using xspec.runner.utility.Notification.Console;

namespace xspec.runner.utility.Writer
{
	public class ConsoleSpecificationWriter : ISpecificationWriter
	{
		private readonly StringBuilder writer = new StringBuilder();

		public ConsoleSpecificationWriter()
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
		}

		public string ReadAll()
		{
			Trace.Flush();
			return writer.ToString();
		}

		public void WriteLine(string message)
		{
			writer.AppendLine(message);
			Trace.WriteLine(message);

			var parser = new Parser.XSpecConsoleTestRunnerOutputParser(
				new ConsoleNotificationService(),
			    message.Split(new string[] { System.Environment.NewLine },
			    StringSplitOptions.RemoveEmptyEntries));
			parser.Parse();
		}
	}
}