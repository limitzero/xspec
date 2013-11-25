using System;
using System.IO;
using System.Linq;
using xSpec.watcher.Internal;
using xSpec.watcher.Modes;
using xspec.watcher.Notifications.Notifiers.Console;
using xspec.watcher.TestRunners.XSpecConsole;

namespace xSpec.watcher.Pipeline
{
	public class TestFilter : IWatchPipelineFilter
	{
		public TestFilter()
		{
			this.Result = string.Empty;
		}

		public void Execute(string current_directory, string current_file)
		{
			if(this.FilterException != null)
				return; // build error, do not continue:

			// extract all test projects from the list of indexed projects:
			var indexes = Indexer.Indexes
				.Where(i => i.Value.IsTestProject)
				.Select(i => i.Value)
				.ToList().Distinct();

			var testProjects = indexes.Select(i => i.ProjectLibraryPath).ToList();

			var notificationService = new ConsoleNotificationService();
			var testRunner = new XSpecConsoleTestRunner(notificationService);
			var buildDirectory = Path.Combine(current_directory, "build"); 

			// run all tests in the test project:
			foreach (var index in indexes)
			{
				//var buildArtifact = Path.Combine(current_directory, string.Concat(@"build\", Path.GetFileName(testProject)));
				testRunner.Run(buildDirectory, Path.GetFileName(index.ProjectLibraryPath));	
			}

			/*
			// extract all test projects from the list of indexed projects:
			var indexes = Indexer.Indexes
				.Where(i => i.Value.IsTestProject)
				.Select(i => i.Value)
				.ToList().Distinct();

			var test_projects = indexes.Select(i => i.ProjectLibraryPath).ToList();

			// run the tests in the extracted test projects:
			Console.WriteLine();
			var extracted_test_runner = new ExtractedTestRunner();
			extracted_test_runner.Run(test_projects);
			Console.WriteLine();
			*/

			this.FilterException = null;
		}

		public string Result { get; set; }
		public Exception FilterException { get; set; }
	}
}