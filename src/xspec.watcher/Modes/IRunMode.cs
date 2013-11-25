using System;
using System.Collections.Specialized;

namespace xSpec.watcher.Modes
{
	public interface IRunMode
	{
		void Execute(StringDictionary commands);
	}
}