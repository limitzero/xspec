using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using watcher.build.test.connector.Internal;
using watcher.core;

namespace watcher.build.test.connector.Filters
{
	public class BuildActionFilter : IWatcherActionFilter
	{
		public WatcherExecuterResult Execute(WatcherExecuterResult watcherExecuterResult,
		                                     FileChangedEvent fileChangedResult)
		{
			var solution_files = Directory.GetFiles(fileChangedResult.WatchedDirectory, "*.sln");

			if (solution_files.Length == 0)
			{
				throw new InvalidOperationException(string.Format("No solution file found on directory {0}",
				                                                  fileChangedResult.Directory));
			}

			var solution_file = solution_files[0];
			FileInfo file = new FileInfo(solution_file);

			ProjectCollection projectCollection = new ProjectCollection();
			Dictionary<string, string> globalProperties = new Dictionary<string, string>();
			globalProperties.Add("Configuration","Debug");
			globalProperties.Add("Platform","x86");
			globalProperties.Add("OutputPath", Path.Combine(file.Directory.FullName, @"\build"));

			BuildRequestData BuidlRequest =new BuildRequestData(file.FullName, globalProperties, 
				null, new string[] { "Build" }, null);

			BuildResult buildResult =
			BuildManager.DefaultBuildManager.Build(new BuildParameters(projectCollection), BuidlRequest);
			
			if(buildResult.Exception != null)
			{
				watcherExecuterResult.HaltOnError = true;
				watcherExecuterResult.ExecuterException = buildResult.Exception;
				Console.WriteLine(buildResult.Exception);
			}

			return watcherExecuterResult;
		}

	}
}