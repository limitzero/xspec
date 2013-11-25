using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace xspec.watcher.BuildRunners.MSBuild
{
	public class MSBuildRunner
	{
		private readonly string project_file_path;

		public MSBuildRunner(string projectFilePath)
		{
			project_file_path = projectFilePath;
		}

		public void Clean(params string[] arguements)
		{
			var lines = new List<string>();
			string buildExecutable = string.Empty;
			Exception loadBuildExecutableException = null;

			if (TryExtractBuildRunnerExecutable(out buildExecutable, out loadBuildExecutableException) == false)
			{
				Console.WriteLine("VS Command Prompt Environment Assumed For Building");
			}

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(buildExecutable);

			if (arguements.Length > 0)
			{
				string args = string.Join(" ", arguements);
				startInfo.Arguments = args;
			}

			startInfo.Arguments += string.Concat(" ", this.project_file_path);

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
		}

		public bool Build(params string[] arguements)
		{
			bool success = true;
			var lines = new List<string>();

			string buildExecutable = string.Empty;
			Exception loadBuildExecutableException = null;

			if(TryExtractBuildRunnerExecutable(out buildExecutable, out loadBuildExecutableException) == false)
			{
				Console.WriteLine("VS Command Prompt Environment Assumed For Building");
			}

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(buildExecutable);

			if (arguements.Length > 0)
			{
				string args = string.Join(" ", arguements);
				startInfo.Arguments = args;
			}

			startInfo.Arguments += string.Concat(" ", this.project_file_path);

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

			var parser = new MSBuildOutputParser(lines);
			parser.Parse(ref success);

			return success;
		}

		private bool TryExtractBuildRunnerExecutable(out string executableFilePath, out Exception loadBuildRunnerExecutableException)
		{
			bool success = false;
			executableFilePath = "msbuild.exe";
			loadBuildRunnerExecutableException = null;

			try
			{
				executableFilePath = System.Configuration.ConfigurationManager.AppSettings.Get("build.executable");
				success = true;
			}
			catch (Exception executableNotProvidedException)
			{
				loadBuildRunnerExecutableException = executableNotProvidedException;	
			}

			return success;
		}
	}
}