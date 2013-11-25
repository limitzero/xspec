using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xspec.runner.utility.Hosting;
using xspec.runner.utility.Runner;

namespace xspec.console
{
	public class ConsoleRunner
	{
		private readonly ICollection<Exception> runner_failures;
		private readonly ISpecificationRunner runner;

		public ConsoleRunner()
		{
			this.runner_failures = new List<Exception>();
			this.runner = new SpecificationRunner();
			this.runner.OnRunnerFailure += RecordRunnerFailure;
		}

		public void RunAssembly(string assemblyName)
		{
			if (string.IsNullOrEmpty(assemblyName))
				throw new ArgumentNullException("library", "The test library name or full path was not specified.");

			// make the change to execute the test library file if the name of the library is used as an input:
			if(assemblyName.EndsWith(".dll") == false)
			{
				assemblyName = string.Concat(assemblyName, ".dll");
			}

			Run(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
		}

		public void RunPath(string pathName)
		{
			if (string.IsNullOrEmpty(pathName))
				throw new ArgumentNullException("path", "The path where the test libraries are located was not specified.");

			if (pathName.Equals("."))
				pathName = System.AppDomain.CurrentDomain.BaseDirectory;

			if (Directory.Exists(pathName) == false)
				throw new InvalidOperationException(string.Format("The directory '{0}' does not exist.", pathName));

			this.runner.RunAllInPath(pathName);
			this.DisplayRunnerFailures();
		}

		private static void Run(string assemblyFilePath, string assemblyFileName)
		{
			var remoteHost = new RemoteExecutorHost(assemblyFilePath, assemblyFileName);
			remoteHost.Execute();
		}

		private void RecordRunnerFailure(Exception runnerFailure)
		{
			this.runner_failures.Add(runnerFailure);
		}

		private void DisplayRunnerFailures()
		{
			if (this.runner_failures.Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine();
				builder.AppendLine("************ FAILURES ************");

				foreach (var failure in runner_failures)
				{
					builder.AppendLine(failure.Message);
				}

				builder.AppendLine();
				Console.WriteLine(builder.ToString());
			}
		}
	}
}