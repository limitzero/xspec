using System.Collections.Generic;
using System.Diagnostics;

namespace xspec.watcher.BuildRunners.Powershell
{
	public class PowershellBuildRunner
	{
		private readonly string solution_file_path;
		private readonly string solution_file_name;

		public PowershellBuildRunner(string solutionFilePath, string solutionFileName)
		{
			solution_file_path = solutionFilePath;
			solution_file_name = solutionFileName;
		}

		public void Build(params string[] arguements)
		{
			var lines = new List<string>();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(@"powershell.exe");

			if (arguements.Length > 0)
			{
				string args = string.Join(" ", arguements);
				startInfo.Arguments = args;
			}

			//startInfo.Arguments += string.Concat(" ", this.project_file_path);

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
	}
}