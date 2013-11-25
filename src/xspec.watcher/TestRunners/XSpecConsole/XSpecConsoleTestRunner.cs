using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using xspec.runner.utility.Hosting;
using xspec.runner.utility.Runner;
using xspec.watcher.Notifications;
using xspec.watcher.Notifications.Messages;

namespace xspec.watcher.TestRunners.XSpecConsole
{
	public class XSpecConsoleTestRunner
	{
		private readonly INotificationService notification_service;

		public XSpecConsoleTestRunner(INotificationService notificationService)
		{
			notification_service = notificationService;
		}

		public void Run(string currentPath, string testLibrary)
		{
			string executable = string.Empty;
			Exception loadTestRunnerException = null;
			if (TryExtractTestRunnerExecutable(out executable, out loadTestRunnerException) == false)
			{
				notification_service.Notify(new FailureNotificationMessage("Test Runner Not Found",
					"There is not a test runner defined."));
				return;
			}

			RunTests(currentPath, executable, testLibrary, string.Empty);
			//UseTestRunnerToInspectTests(currentPath, executable, testLibrary);
		}

		private void RunTests(string currentPath, string executable, string testLibrary, params string[] arguements)
		{
			var lines = new List<string>();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(executable);

			if (arguements.Length > 0)
			{
				string args = string.Join(" ", arguements);
				startInfo.Arguments = args;
			}

			startInfo.Arguments += string.Concat(" ", testLibrary);

			startInfo.WorkingDirectory = currentPath;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			
			using (var process = new Process())
			{
				process.StartInfo = startInfo;
				process.Start();

				string line;
				while ((line = process.StandardOutput.ReadLine()) != null)
				{
					lines.Add(line);
				}

				process.WaitForExit();
			}

			var parser = new XSpecConsoleTestRunnerOutputParser(notification_service, lines);
			parser.Parse();
		}

		private void UseTestRunnerToInspectTests(string currentPath, string executable, string testLibrary)
		{
			// execute the tests in a different app domain (for clean and build cycle to work effectively):
			var remote_host = new RemoteExecutorHost(currentPath, testLibrary);
			//remote_host.Execute();

			using (var host = new RemoteExecutorHost(currentPath, testLibrary))
			{
				host.Execute();
			}

			return;

			// run the tests from the test project in the current output directory:
			ISpecificationRunner runner = new SpecificationRunner();
			var path = Path.Combine(currentPath, testLibrary);

			//Assembly assembly = Assembly.LoadFrom(path);
			//runner.RunAssembly(assembly);
			//assembly = null;

			//var lines =
			//    new List<string>(runner.Writer.ReadAll().Split(new string[] { System.Environment.NewLine },
			//                                                   StringSplitOptions.RemoveEmptyEntries));

			//var parser = new XSpecConsoleTestRunnerOutputParser(notification_service, lines);
			//parser.Parse();
		}

		private static bool CopyTestsRunnerToBuildDirectory(string currentPath, string executablePath, string testLibrary)
		{
			var targetPath = Path.Combine(currentPath, @"build");

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(@"xcopy.exe");
			startInfo.Arguments = string.Format("{0} {1} /Y", executablePath, targetPath);
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;

			using (var process = new Process())
			{
				process.StartInfo = startInfo;
				process.Start();
			}

			return File.Exists(Path.Combine(targetPath, Path.GetFileName(executablePath)));
		}

		private static bool TryExtractTestRunnerExecutable(out string executable, out Exception loadTestRunerExecutableException)
		{
			bool success = false;
			executable = string.Empty;
			loadTestRunerExecutableException = null;

			try
			{
				executable = System.Configuration.ConfigurationManager.AppSettings.Get("testrunner.executable");
				success = string.IsNullOrEmpty(executable) == false;
			}
			catch (Exception tryGetExecutablePathException)
			{
				loadTestRunerExecutableException = tryGetExecutablePathException;
			}

			return success;
		}
	}
}