using System.Diagnostics;

namespace watcher.build.test.connector.Internal
{
	public class CommandFileExecutor
	{
		public string Execute(string command_file, params string[] arguments)
		{
			string result;
			
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = string.Format(@"{0}", command_file);

			if (arguments.Length > 0)
			{
				string args = string.Join(" ", arguments);
				startInfo.Arguments = args;
			}

			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;

			using (var process = new Process())
			{
				process.StartInfo = startInfo;
				process.Start();
				result = process.StandardOutput.ReadToEnd();
			}

			return result;
		}
	}
}