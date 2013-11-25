using System;
using System.IO;
using System.Linq;
using xspec.watcher.BuildRunners.MSBuild;
using xSpec.watcher.Internal;
using xSpec.watcher.Modes;

namespace xSpec.watcher.Pipeline
{
	public class BuildFilter : IWatchPipelineFilter
	{
		public string Result { get; set; }
		public Exception FilterException { get; set; }

		public BuildFilter()
		{
			this.Result = string.Empty;
		}

		public void Execute(string current_directory, string current_file)
		{
			// find the index of the project where the item change was done:
			var index = Indexer.Indexes
				.Where(i => i.Value.ClassFiles.FirstOrDefault(cf => current_file.Equals(cf)) != null)
				.Select(i => i.Value)
				.FirstOrDefault();

			if (index == null) return;

			Console.WriteLine("building project: " + index.ProjectFilePath);

			// for msbuild, you must first do a clean then a build to simulate a rebuild action:
			var msbuild = new MSBuildRunner(Indexer.Solution); 

			// clean the solution:
			var clean_arguements = string.Format(@"/t:clean /v:quiet");
			msbuild.Clean(clean_arguements);

			// build the solution:
			var arguements = string.Format(@"/t:build /property:WarningLevel=0,OutDir={0}\ /v:quiet", Path.Combine(current_directory,"build"));
			var result = msbuild.Build(arguements);

			// check the results of the build:
			this.FilterException = result ? null : new Exception("Build Failed");
		}
	}
}