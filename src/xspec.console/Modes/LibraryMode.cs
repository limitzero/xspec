using System.Collections.Specialized;
using xspec.console.Runner;
using xspec.runner.utility.Settings;

namespace xspec.console.Modes
{
	public class LibraryMode : IRunMode
	{
		public void Execute(ExecutingOptions executingOptions)
		{
			new ConsoleRunner().RunAssembly(executingOptions.Library);
		}
	}
}