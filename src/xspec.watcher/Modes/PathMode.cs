using System.Collections.Generic;
using System.Collections.Specialized;
using xspec.watcher;

namespace xSpec.watcher.Modes
{
	public class PathMode : IRunMode
	{
		public void Execute(StringDictionary commands)
		{
			var path = commands[CommandOptions.Path.ToString().ToLower()];

			if (string.IsNullOrEmpty(path) || path.Equals("true"))
				path = "."; // force it to look in the local AppDomain /bin directory

			new ConsoleRunner().RunPath(path);
		}
	}
}