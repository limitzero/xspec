using System.Collections.Specialized;
using WPFGrowlNotification.Runner;

namespace WPFGrowlNotification.Modes
{
	public interface IRunMode
	{
		Notification Execute(ExecutingOptions executingOptions);
	}
}