using System.Collections.Generic;

namespace xSpec.watcher.Pipeline
{
	public interface IExtractedTestRunner
	{
		void Run(IEnumerable<string> testLibraries);
	}
}